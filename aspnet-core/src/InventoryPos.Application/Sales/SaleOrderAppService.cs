using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Catalog;
using InventoryPos.Permissions;
using InventoryPos.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace InventoryPos.Sales;

public class SaleOrderAppService : InventoryPosAppService, ISaleOrderAppService
{
    private readonly IRepository<SaleOrder, Guid> _saleOrderRepository;
    private readonly IRepository<SaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public SaleOrderAppService(
        IRepository<SaleOrder, Guid> saleOrderRepository,
        IRepository<SaleReturn, Guid> saleReturnRepository,
        IRepository<Product, Guid> productRepository)
    {
        _saleOrderRepository = saleOrderRepository;
        _saleReturnRepository = saleReturnRepository;
        _productRepository = productRepository;
    }

    [Authorize(InventoryPosPermissions.SaleOrders.Create)]
    public async Task<SaleOrderDto> CreateAsync(CreateSaleOrderDto input)
    {
        var order = new SaleOrder();

        foreach (var itemInput in input.Items)
        {
            var product = await _productRepository.GetAsync(itemInput.ProductId);

            if (product.QuantityOnHand < itemInput.Quantity)
            {
                throw new UserFriendlyException(
                    $"Not enough stock for '{product.Name}'. On hand: {product.QuantityOnHand}, requested: {itemInput.Quantity}.");
            }

            product.QuantityOnHand -= itemInput.Quantity;
            await _productRepository.UpdateAsync(product);

            order.Items.Add(new SaleOrderItem
            {
                ProductId = product.Id,
                Quantity = itemInput.Quantity,
                UnitPrice = product.Price
            });
        }

        var taxRate = await GetTaxRatePercentAsync();
        ApplyPricing(order, input, taxRate);

        order = await _saleOrderRepository.InsertAsync(order);

        return (await MapOrdersAsync(new List<SaleOrder> { order })).Single();
    }

    [Authorize(InventoryPosPermissions.SaleOrders.Default)]
    public async Task<SaleOrderDto> GetAsync(Guid id)
    {
        var queryable = await _saleOrderRepository.WithDetailsAsync(x => x.Items);
        var order = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id));
        if (order == null)
        {
            throw new UserFriendlyException("Sale not found.");
        }

        return (await MapOrdersAsync(new List<SaleOrder> { order })).Single();
    }

    [Authorize(InventoryPosPermissions.SaleOrders.Default)]
    public async Task<PagedResultDto<SaleOrderDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _saleOrderRepository.WithDetailsAsync(x => x.Items);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var orders = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<SaleOrderDto>(totalCount, await MapOrdersAsync(orders));
    }

    [Authorize(InventoryPosPermissions.SaleOrders.Create)]
    public async Task<PricingInfoDto> GetPricingInfoAsync()
    {
        return new PricingInfoDto { TaxRatePercent = await GetTaxRatePercentAsync() };
    }

    private async Task<decimal> GetTaxRatePercentAsync()
    {
        var rate = await SettingProvider.GetAsync<decimal>(InventoryPosSettings.TaxRatePercent);
        return Math.Clamp(rate, 0m, 100m);
    }

    private static void ApplyPricing(SaleOrder order, CreateSaleOrderDto input, decimal taxRatePercent)
    {
        order.Subtotal = order.GetItemsSubtotal();

        order.DiscountType = input.DiscountType;
        order.DiscountValue = input.DiscountType == DiscountType.None ? 0 : input.DiscountValue;

        switch (input.DiscountType)
        {
            case DiscountType.None:
                order.DiscountAmount = 0;
                break;
            case DiscountType.Percent:
                if (input.DiscountValue < 0 || input.DiscountValue > 100)
                {
                    throw new UserFriendlyException("A percentage discount must be between 0 and 100.");
                }

                order.DiscountAmount = Math.Round(order.Subtotal * input.DiscountValue / 100m, 2, MidpointRounding.AwayFromZero);
                break;
            case DiscountType.Amount:
                if (input.DiscountValue < 0)
                {
                    throw new UserFriendlyException("A discount cannot be negative.");
                }

                order.DiscountAmount = Math.Min(Math.Round(input.DiscountValue, 2, MidpointRounding.AwayFromZero), order.Subtotal);
                break;
            default:
                throw new UserFriendlyException("Unknown discount type.");
        }

        var taxable = order.Subtotal - order.DiscountAmount;
        order.TaxRatePercent = taxRatePercent;
        order.TaxAmount = Math.Round(taxable * taxRatePercent / 100m, 2, MidpointRounding.AwayFromZero);
        order.TotalAmount = taxable + order.TaxAmount;

        order.PaymentMethod = input.PaymentMethod;
        if (input.PaymentMethod == PaymentMethod.Card)
        {
            order.AmountTendered = order.TotalAmount;
        }
        else
        {
            var tendered = input.AmountTendered ?? order.TotalAmount;
            if (tendered < order.TotalAmount)
            {
                throw new UserFriendlyException(
                    $"Cash received ({tendered:0.00}) is less than the total due ({order.TotalAmount:0.00}).");
            }

            order.AmountTendered = tendered;
        }
    }

    private async Task<List<SaleOrderDto>> MapOrdersAsync(List<SaleOrder> orders)
    {
        var orderIds = orders.Select(o => o.Id).ToList();
        var returns = await AsyncExecuter.ToListAsync(
            (await _saleReturnRepository.GetQueryableAsync()).Where(r => orderIds.Contains(r.SaleOrderId)));
        var refundedByOrder = returns
            .GroupBy(r => r.SaleOrderId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.RefundAmount));

        var dtos = ObjectMapper.Map<List<SaleOrder>, List<SaleOrderDto>>(orders);
        foreach (var dto in dtos)
        {
            dto.RefundedAmount = refundedByOrder.GetValueOrDefault(dto.Id);

            // Sales recorded before discounts/tax existed have no stored subtotal; they had neither, so total == subtotal.
            if (dto.Subtotal == 0 && dto.TotalAmount > 0)
            {
                dto.Subtotal = dto.TotalAmount;
                dto.AmountTendered = dto.TotalAmount;
            }
        }

        return dtos;
    }
}
