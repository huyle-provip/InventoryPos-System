using System;
using System.Threading.Tasks;
using InventoryPos.Catalog;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace InventoryPos.Stocks;

public abstract class StockTransactionAppServiceTests<TStartupModule> : InventoryPosApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IStockTransactionAppService _stockAppService;
    private readonly IRepository<Product, Guid> _productRepository;

    protected StockTransactionAppServiceTests()
    {
        _stockAppService = GetRequiredService<IStockTransactionAppService>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
    }

    private async Task<Guid> CreateProductAsync(int quantity)
    {
        var product = new Product { Sku = "SKU-" + Guid.NewGuid().ToString("N")[..8], Name = "Test", Price = 1m, QuantityOnHand = quantity };
        await WithUnitOfWorkAsync(async () => await _productRepository.InsertAsync(product, autoSave: true));
        return product.Id;
    }

    private Task<int> GetQuantityAsync(Guid id) =>
        WithUnitOfWorkAsync(async () => (await _productRepository.GetAsync(id)).QuantityOnHand);

    [Fact]
    public async Task Stock_In_Increases_Quantity()
    {
        var id = await CreateProductAsync(5);

        await _stockAppService.CreateAsync(new CreateStockTransactionDto { ProductId = id, Type = StockTransactionType.In, Quantity = 7 });

        (await GetQuantityAsync(id)).ShouldBe(12);
    }

    [Fact]
    public async Task Stock_Out_Decreases_Quantity()
    {
        var id = await CreateProductAsync(5);

        await _stockAppService.CreateAsync(new CreateStockTransactionDto { ProductId = id, Type = StockTransactionType.Out, Quantity = 2 });

        (await GetQuantityAsync(id)).ShouldBe(3);
    }

    [Fact]
    public async Task Stock_Out_Beyond_On_Hand_Is_Rejected()
    {
        var id = await CreateProductAsync(1);

        await Should.ThrowAsync<UserFriendlyException>(() =>
            _stockAppService.CreateAsync(new CreateStockTransactionDto { ProductId = id, Type = StockTransactionType.Out, Quantity = 2 }));

        (await GetQuantityAsync(id)).ShouldBe(1);
    }
}
