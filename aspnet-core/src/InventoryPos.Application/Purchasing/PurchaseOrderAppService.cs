using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Catalog;
using InventoryPos.Permissions;
using InventoryPos.Stocks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Purchasing;

public class PurchaseOrderAppService : InventoryPosAppService, IPurchaseOrderAppService
{
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<StockTransaction, Guid> _stockTransactionRepository;

    public PurchaseOrderAppService(
        IRepository<PurchaseOrder, Guid> purchaseOrderRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<StockTransaction, Guid> stockTransactionRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _stockTransactionRepository = stockTransactionRepository;
    }

    [Authorize(InventoryPosPermissions.PurchaseOrders.Create)]
    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input)
    {
        if (!await _supplierRepository.AnyAsync(s => s.Id == input.SupplierId))
        {
            throw new UserFriendlyException("Supplier not found.");
        }

        var order = new PurchaseOrder
        {
            OrderNumber = NewOrderNumber(),
            SupplierId = input.SupplierId,
            Note = input.Note,
            Status = PurchaseOrderStatus.Open
        };

        foreach (var line in input.Items)
        {
            if (!await _productRepository.AnyAsync(p => p.Id == line.ProductId))
            {
                throw new UserFriendlyException("One of the products no longer exists.");
            }

            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = line.ProductId,
                QuantityOrdered = line.Quantity,
                UnitCost = line.UnitCost
            });
        }

        order.TotalCost = order.Items.Sum(i => i.QuantityOrdered * i.UnitCost);
        order = await _purchaseOrderRepository.InsertAsync(order);

        return (await MapAsync(new List<PurchaseOrder> { order })).Single();
    }

    [Authorize(InventoryPosPermissions.PurchaseOrders.Default)]
    public async Task<PurchaseOrderDto> GetAsync(Guid id)
    {
        var order = await LoadOrderAsync(id);
        return (await MapAsync(new List<PurchaseOrder> { order })).Single();
    }

    [Authorize(InventoryPosPermissions.PurchaseOrders.Default)]
    public async Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input)
    {
        var queryable = await _purchaseOrderRepository.WithDetailsAsync(x => x.Items);

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.ReceivableOnly == true)
        {
            queryable = queryable.Where(x =>
                x.Status == PurchaseOrderStatus.Open || x.Status == PurchaseOrderStatus.PartiallyReceived);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var orders = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<PurchaseOrderDto>(totalCount, await MapAsync(orders));
    }

    [Authorize(InventoryPosPermissions.PurchaseOrders.Receive)]
    public async Task<PurchaseOrderDto> ReceiveAsync(Guid id, ReceivePurchaseOrderDto input)
    {
        var order = await LoadOrderAsync(id);

        if (order.Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Received)
        {
            throw new UserFriendlyException($"Purchase order {order.OrderNumber} is {order.Status} and cannot receive more stock.");
        }

        var lines = input.Items
            .Where(i => i.Quantity > 0)
            .GroupBy(i => i.PurchaseOrderItemId)
            .Select(g => new { ItemId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        if (lines.Count == 0)
        {
            throw new UserFriendlyException("Enter a received quantity for at least one item.");
        }

        foreach (var line in lines)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == line.ItemId);
            if (item == null)
            {
                throw new UserFriendlyException("One of the items does not belong to this purchase order.");
            }

            var outstanding = item.QuantityOrdered - item.QuantityReceived;
            if (line.Quantity > outstanding)
            {
                throw new UserFriendlyException($"Only {outstanding} unit(s) are still outstanding for an item on {order.OrderNumber}.");
            }

            item.QuantityReceived += line.Quantity;

            var product = await _productRepository.GetAsync(item.ProductId);
            product.QuantityOnHand += line.Quantity;
            await _productRepository.UpdateAsync(product);

            await _stockTransactionRepository.InsertAsync(new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.In,
                Quantity = line.Quantity,
                Note = $"Received against {order.OrderNumber}"
            });
        }

        order.RefreshStatusFromItems();
        await _purchaseOrderRepository.UpdateAsync(order);

        return (await MapAsync(new List<PurchaseOrder> { order })).Single();
    }

    [Authorize(InventoryPosPermissions.PurchaseOrders.Cancel)]
    public async Task<PurchaseOrderDto> CancelAsync(Guid id)
    {
        var order = await LoadOrderAsync(id);

        if (order.Status != PurchaseOrderStatus.Open)
        {
            throw new UserFriendlyException("Only a purchase order with nothing received yet can be cancelled.");
        }

        order.Status = PurchaseOrderStatus.Cancelled;
        await _purchaseOrderRepository.UpdateAsync(order);

        return (await MapAsync(new List<PurchaseOrder> { order })).Single();
    }

    private async Task<PurchaseOrder> LoadOrderAsync(Guid id)
    {
        var queryable = await _purchaseOrderRepository.WithDetailsAsync(x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (order == null)
        {
            throw new UserFriendlyException("Purchase order not found.");
        }

        return order;
    }

    private static string NewOrderNumber()
    {
        return $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
    }

    private async Task<List<PurchaseOrderDto>> MapAsync(List<PurchaseOrder> orders)
    {
        var supplierIds = orders.Select(o => o.SupplierId).Distinct().ToList();
        var productIds = orders.SelectMany(o => o.Items).Select(i => i.ProductId).Distinct().ToList();

        var suppliers = (await _supplierRepository.GetListAsync(s => supplierIds.Contains(s.Id)))
            .ToDictionary(s => s.Id, s => s.Name);
        var products = (await _productRepository.GetListAsync(p => productIds.Contains(p.Id)))
            .ToDictionary(p => p.Id);

        var dtos = ObjectMapper.Map<List<PurchaseOrder>, List<PurchaseOrderDto>>(orders);
        foreach (var dto in dtos)
        {
            dto.SupplierName = suppliers.GetValueOrDefault(dto.SupplierId, "(deleted supplier)");
            foreach (var item in dto.Items)
            {
                if (products.TryGetValue(item.ProductId, out var product))
                {
                    item.ProductName = product.Name;
                    item.Sku = product.Sku;
                }
            }
        }

        return dtos;
    }
}
