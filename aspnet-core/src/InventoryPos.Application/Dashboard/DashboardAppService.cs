using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Catalog;
using InventoryPos.Purchasing;
using InventoryPos.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Dashboard;

[Authorize]
public class DashboardAppService : InventoryPosAppService, IDashboardAppService
{
    private const int TopProductsCount = 5;
    private const int LowStockListCount = 10;

    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<SaleOrder, Guid> _saleOrderRepository;
    private readonly IRepository<SaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepository;

    public DashboardAppService(
        IRepository<Product, Guid> productRepository,
        IRepository<SaleOrder, Guid> saleOrderRepository,
        IRepository<SaleReturn, Guid> saleReturnRepository,
        IRepository<PurchaseOrder, Guid> purchaseOrderRepository)
    {
        _productRepository = productRepository;
        _saleOrderRepository = saleOrderRepository;
        _saleReturnRepository = saleReturnRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<DashboardDto> GetAsync()
    {
        var today = Clock.Now.Date;
        var weekStart = today.AddDays(-6);
        var monthStart = today.AddDays(-29);

        var products = await _productRepository.GetListAsync();

        var recentOrdersQuery = await _saleOrderRepository.WithDetailsAsync(x => x.Items);
        var recentOrders = await AsyncExecuter.ToListAsync(
            recentOrdersQuery.Where(x => x.CreationTime >= monthStart));

        var recentReturns = await _saleReturnRepository.GetListAsync(x => x.CreationTime >= monthStart);

        var todayOrders = recentOrders.Where(x => x.CreationTime >= today).ToList();
        var todayRefunds = recentReturns.Where(r => r.CreationTime >= today).Sum(r => r.RefundAmount);

        // Refunds reduce the day they were issued, not the day of the original sale.
        var salesLast7Days = Enumerable.Range(0, 7)
            .Select(offset => weekStart.AddDays(offset))
            .Select(day =>
            {
                var dayOrders = recentOrders.Where(o => o.CreationTime.Date == day).ToList();
                var dayRefunds = recentReturns.Where(r => r.CreationTime.Date == day).Sum(r => r.RefundAmount);
                return new DailySalesDto
                {
                    Date = day,
                    Total = dayOrders.Sum(o => o.TotalAmount) - dayRefunds,
                    OrderCount = dayOrders.Count
                };
            })
            .ToList();

        var openPurchaseOrders = await _purchaseOrderRepository.CountAsync(x =>
            x.Status == PurchaseOrderStatus.Open || x.Status == PurchaseOrderStatus.PartiallyReceived);

        var productNames = products.ToDictionary(p => p.Id, p => p.Name);

        var topProducts = recentOrders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = productNames.GetValueOrDefault(g.Key, "(deleted product)"),
                QuantitySold = g.Sum(i => i.Quantity - i.ReturnedQuantity),
                Revenue = g.Sum(i => (i.Quantity - i.ReturnedQuantity) * i.UnitPrice)
            })
            .Where(t => t.QuantitySold > 0)
            .OrderByDescending(t => t.QuantitySold)
            .Take(TopProductsCount)
            .ToList();

        var lowStock = products
            .Where(p => p.QuantityOnHand <= p.ReorderThreshold)
            .OrderBy(p => p.QuantityOnHand)
            .ToList();

        return new DashboardDto
        {
            TodaySalesTotal = todayOrders.Sum(o => o.TotalAmount) - todayRefunds,
            TodayOrderCount = todayOrders.Count,
            TodayRefundTotal = todayRefunds,
            OpenPurchaseOrderCount = openPurchaseOrders,
            TotalProducts = products.Count,
            LowStockCount = lowStock.Count,
            InventoryValue = products.Sum(p => p.QuantityOnHand * p.Price),
            SalesLast7Days = salesLast7Days,
            TopProducts = topProducts,
            LowStockProducts = lowStock
                .Take(LowStockListCount)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    QuantityOnHand = p.QuantityOnHand,
                    ReorderThreshold = p.ReorderThreshold
                })
                .ToList()
        };
    }
}
