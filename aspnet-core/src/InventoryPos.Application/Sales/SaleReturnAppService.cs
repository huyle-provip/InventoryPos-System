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

namespace InventoryPos.Sales;

public class SaleReturnAppService : InventoryPosAppService, ISaleReturnAppService
{
    private readonly IRepository<SaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<SaleOrder, Guid> _saleOrderRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<StockTransaction, Guid> _stockTransactionRepository;

    public SaleReturnAppService(
        IRepository<SaleReturn, Guid> saleReturnRepository,
        IRepository<SaleOrder, Guid> saleOrderRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<StockTransaction, Guid> stockTransactionRepository)
    {
        _saleReturnRepository = saleReturnRepository;
        _saleOrderRepository = saleOrderRepository;
        _productRepository = productRepository;
        _stockTransactionRepository = stockTransactionRepository;
    }

    [Authorize(InventoryPosPermissions.SaleReturns.Create)]
    public async Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto input)
    {
        var ordersQuery = await _saleOrderRepository.WithDetailsAsync(x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(ordersQuery.Where(x => x.Id == input.SaleOrderId));
        if (order == null)
        {
            throw new UserFriendlyException("Sale not found.");
        }

        var requested = input.Items
            .GroupBy(i => i.SaleOrderItemId)
            .Select(g => new { SaleOrderItemId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        // Each refunded unit gets back its proportional share of the sale's discount and tax.
        var itemsSubtotal = order.GetItemsSubtotal();
        var ratio = itemsSubtotal > 0 ? order.TotalAmount / itemsSubtotal : 1m;

        var saleReturn = new SaleReturn { SaleOrderId = order.Id, Reason = input.Reason };

        foreach (var line in requested)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.Id == line.SaleOrderItemId);
            if (orderItem == null)
            {
                throw new UserFriendlyException("One of the items does not belong to this sale.");
            }

            var returnable = orderItem.Quantity - orderItem.ReturnedQuantity;
            if (line.Quantity > returnable)
            {
                throw new UserFriendlyException($"Only {returnable} unit(s) of an item can still be returned.");
            }

            orderItem.ReturnedQuantity += line.Quantity;

            var product = await _productRepository.GetAsync(orderItem.ProductId);
            product.QuantityOnHand += line.Quantity;
            await _productRepository.UpdateAsync(product);

            await _stockTransactionRepository.InsertAsync(new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.In,
                Quantity = line.Quantity,
                Note = $"Return of sale {order.Id.ToString()[..8]}"
            });

            saleReturn.Items.Add(new SaleReturnItem
            {
                SaleOrderItemId = orderItem.Id,
                ProductId = orderItem.ProductId,
                Quantity = line.Quantity,
                RefundAmount = Math.Round(line.Quantity * orderItem.UnitPrice * ratio, 2, MidpointRounding.AwayFromZero)
            });
        }

        saleReturn.RefundAmount = saleReturn.Items.Sum(i => i.RefundAmount);

        // When this return clears the last outstanding units, refund the exact remainder so rounding never leaves cents behind.
        if (order.Items.All(i => i.ReturnedQuantity >= i.Quantity))
        {
            var previouslyRefunded = (await _saleReturnRepository.GetListAsync(r => r.SaleOrderId == order.Id))
                .Sum(r => r.RefundAmount);
            var exactRemainder = order.TotalAmount - previouslyRefunded;
            var difference = exactRemainder - saleReturn.RefundAmount;

            saleReturn.Items.Last().RefundAmount += difference;
            saleReturn.RefundAmount = exactRemainder;
        }

        await _saleOrderRepository.UpdateAsync(order);
        saleReturn = await _saleReturnRepository.InsertAsync(saleReturn);

        return ObjectMapper.Map<SaleReturn, SaleReturnDto>(saleReturn);
    }

    [Authorize(InventoryPosPermissions.SaleReturns.Default)]
    public async Task<PagedResultDto<SaleReturnDto>> GetListAsync(GetSaleReturnListDto input)
    {
        var queryable = await _saleReturnRepository.WithDetailsAsync(x => x.Items);

        if (input.SaleOrderId.HasValue)
        {
            queryable = queryable.Where(x => x.SaleOrderId == input.SaleOrderId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var returns = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<SaleReturnDto>(
            totalCount,
            ObjectMapper.Map<List<SaleReturn>, List<SaleReturnDto>>(returns));
    }
}
