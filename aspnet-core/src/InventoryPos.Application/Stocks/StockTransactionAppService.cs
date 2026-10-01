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

namespace InventoryPos.Stocks;

public class StockTransactionAppService : InventoryPosAppService, IStockTransactionAppService
{
    private readonly IRepository<StockTransaction, Guid> _stockTransactionRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public StockTransactionAppService(
        IRepository<StockTransaction, Guid> stockTransactionRepository,
        IRepository<Product, Guid> productRepository)
    {
        _stockTransactionRepository = stockTransactionRepository;
        _productRepository = productRepository;
    }

    [Authorize(InventoryPosPermissions.StockTransactions.Create)]
    public async Task<StockTransactionDto> CreateAsync(CreateStockTransactionDto input)
    {
        var product = await _productRepository.GetAsync(input.ProductId);

        if (input.Type == StockTransactionType.Out && product.QuantityOnHand < input.Quantity)
        {
            throw new UserFriendlyException(
                $"Not enough stock for '{product.Name}'. On hand: {product.QuantityOnHand}, requested: {input.Quantity}.");
        }

        product.QuantityOnHand += input.Type == StockTransactionType.In ? input.Quantity : -input.Quantity;
        await _productRepository.UpdateAsync(product);

        var transaction = new StockTransaction
        {
            ProductId = input.ProductId,
            Type = input.Type,
            Quantity = input.Quantity,
            Note = input.Note
        };
        transaction = await _stockTransactionRepository.InsertAsync(transaction);

        return ObjectMapper.Map<StockTransaction, StockTransactionDto>(transaction);
    }

    [Authorize(InventoryPosPermissions.StockTransactions.Default)]
    public async Task<PagedResultDto<StockTransactionDto>> GetListAsync(GetStockTransactionListDto input)
    {
        var queryable = await _stockTransactionRepository.GetQueryableAsync();

        if (input.ProductId.HasValue)
        {
            queryable = queryable.Where(x => x.ProductId == input.ProductId.Value);
        }

        var totalCount = queryable.Count();
        var items = queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<StockTransactionDto>(
            totalCount,
            ObjectMapper.Map<List<StockTransaction>, List<StockTransactionDto>>(items));
    }
}
