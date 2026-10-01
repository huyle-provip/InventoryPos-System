using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Catalog;
using InventoryPos.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Sales;

public class SaleOrderAppService : InventoryPosAppService, ISaleOrderAppService
{
    private readonly IRepository<SaleOrder, Guid> _saleOrderRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public SaleOrderAppService(
        IRepository<SaleOrder, Guid> saleOrderRepository,
        IRepository<Product, Guid> productRepository)
    {
        _saleOrderRepository = saleOrderRepository;
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

        order.TotalAmount = order.Items.Sum(x => x.Quantity * x.UnitPrice);

        order = await _saleOrderRepository.InsertAsync(order);

        return ObjectMapper.Map<SaleOrder, SaleOrderDto>(order);
    }

    [Authorize(InventoryPosPermissions.SaleOrders.Default)]
    public async Task<PagedResultDto<SaleOrderDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _saleOrderRepository.WithDetailsAsync(x => x.Items);

        var totalCount = queryable.Count();
        var items = queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<SaleOrderDto>(
            totalCount,
            ObjectMapper.Map<List<SaleOrder>, List<SaleOrderDto>>(items));
    }
}
