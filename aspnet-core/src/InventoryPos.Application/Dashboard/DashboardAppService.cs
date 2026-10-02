using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Catalog;
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

    public DashboardAppService(
        IRepository<Product, Guid> productRepository,
        IRepository<SaleOrder, Guid> saleOrderRepository)
    {
        _productRepository = productRepository;
        _saleOrderRepository = saleOrderRepository;
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

        var todayOrders = recentOrders.Where(x => x.CreationTime >= today).ToList();

        var salesLast7Days = Enumerable.Range(0, 7)
            .Select(offset => weekStart.AddDays(offset))
            .Select(day =>
            {
                var dayOrders = recentOrders.Where(o => o.CreationTime.Date == day).ToList();
                return new DailySalesDto
                {
                    Date = day,
                    Total = dayOrders.Sum(o => o.TotalAmount),
                    OrderCount = dayOrders.Count
                };
            })
            .ToList();

        var productNames = products.ToDictionary(p => p.Id, p => p.Name);

        var topProducts = recentOrders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = productNames.GetValueOrDefault(g.Key, "(deleted product)"),
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(t => t.QuantitySold)
            .Take(TopProductsCount)
            .ToList();

        var lowStock = products
            .Where(p => p.QuantityOnHand <= p.ReorderThreshold)
            .OrderBy(p => p.QuantityOnHand)
            .ToList();

        return new DashboardDto
        {
            TodaySalesTotal = todayOrders.Sum(o => o.TotalAmount),
            TodayOrderCount = todayOrders.Count,
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
