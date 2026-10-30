using System;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Catalog;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace InventoryPos.Sales;

public abstract class SaleOrderAppServiceTests<TStartupModule> : InventoryPosApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ISaleOrderAppService _saleOrderAppService;
    private readonly ISaleReturnAppService _saleReturnAppService;
    private readonly IRepository<Product, Guid> _productRepository;

    protected SaleOrderAppServiceTests()
    {
        _saleOrderAppService = GetRequiredService<ISaleOrderAppService>();
        _saleReturnAppService = GetRequiredService<ISaleReturnAppService>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
    }

    private async Task<Guid> CreateProductAsync(decimal price = 10m, int quantity = 100)
    {
        var product = new Product
        {
            Sku = "SKU-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Test product",
            Price = price,
            QuantityOnHand = quantity
        };
        await WithUnitOfWorkAsync(async () => await _productRepository.InsertAsync(product, autoSave: true));
        return product.Id;
    }

    private async Task<int> GetQuantityOnHandAsync(Guid productId)
    {
        return await WithUnitOfWorkAsync(async () => (await _productRepository.GetAsync(productId)).QuantityOnHand);
    }

    private static CreateSaleOrderDto SaleOf(Guid productId, int quantity) => new()
    {
        Items = { new CreateSaleOrderItemDto { ProductId = productId, Quantity = quantity } }
    };

    [Fact]
    public async Task Sale_Without_Discount_Applies_Default_Ten_Percent_Tax()
    {
        var productId = await CreateProductAsync(price: 10m);

        var sale = await _saleOrderAppService.CreateAsync(SaleOf(productId, 3));

        sale.Subtotal.ShouldBe(30m);
        sale.DiscountAmount.ShouldBe(0m);
        sale.TaxAmount.ShouldBe(3m);
        sale.TotalAmount.ShouldBe(33m);
    }

    [Fact]
    public async Task Percent_Discount_Is_Applied_Before_Tax()
    {
        var productId = await CreateProductAsync(price: 10m);
        var input = SaleOf(productId, 3);
        input.DiscountType = DiscountType.Percent;
        input.DiscountValue = 10m;

        var sale = await _saleOrderAppService.CreateAsync(input);

        sale.DiscountAmount.ShouldBe(3m);
        sale.TaxAmount.ShouldBe(2.70m);
        sale.TotalAmount.ShouldBe(29.70m);
    }

    [Fact]
    public async Task Amount_Discount_Is_Capped_At_Subtotal()
    {
        var productId = await CreateProductAsync(price: 10m);
        var input = SaleOf(productId, 1);
        input.DiscountType = DiscountType.Amount;
        input.DiscountValue = 50m;

        var sale = await _saleOrderAppService.CreateAsync(input);

        sale.DiscountAmount.ShouldBe(10m);
        sale.TotalAmount.ShouldBe(0m);
    }

    [Fact]
    public async Task Percent_Discount_Over_100_Is_Rejected()
    {
        var productId = await CreateProductAsync();
        var input = SaleOf(productId, 1);
        input.DiscountType = DiscountType.Percent;
        input.DiscountValue = 101m;

        await Should.ThrowAsync<UserFriendlyException>(() => _saleOrderAppService.CreateAsync(input));
    }

    [Fact]
    public async Task Cash_Less_Than_Total_Is_Rejected()
    {
        var productId = await CreateProductAsync(price: 10m);
        var input = SaleOf(productId, 1);
        input.PaymentMethod = PaymentMethod.Cash;
        input.AmountTendered = 5m;

        await Should.ThrowAsync<UserFriendlyException>(() => _saleOrderAppService.CreateAsync(input));
    }

    [Fact]
    public async Task Card_Payment_Tenders_Exact_Total()
    {
        var productId = await CreateProductAsync(price: 10m);
        var input = SaleOf(productId, 1);
        input.PaymentMethod = PaymentMethod.Card;

        var sale = await _saleOrderAppService.CreateAsync(input);

        sale.AmountTendered.ShouldBe(sale.TotalAmount);
    }

    [Fact]
    public async Task Sale_Reduces_Stock()
    {
        var productId = await CreateProductAsync(quantity: 10);

        await _saleOrderAppService.CreateAsync(SaleOf(productId, 4));

        (await GetQuantityOnHandAsync(productId)).ShouldBe(6);
    }

    [Fact]
    public async Task Selling_More_Than_On_Hand_Is_Rejected_And_Stock_Is_Unchanged()
    {
        var productId = await CreateProductAsync(quantity: 2);

        await Should.ThrowAsync<UserFriendlyException>(() => _saleOrderAppService.CreateAsync(SaleOf(productId, 3)));

        (await GetQuantityOnHandAsync(productId)).ShouldBe(2);
    }

    [Fact]
    public async Task Partial_Then_Full_Refund_Returns_Stock_And_Exact_Total()
    {
        var productId = await CreateProductAsync(price: 10m, quantity: 10);
        var input = SaleOf(productId, 3);
        input.DiscountType = DiscountType.Percent;
        input.DiscountValue = 10m;
        var sale = await _saleOrderAppService.CreateAsync(input);
        var itemId = sale.Items.Single().Id;

        var first = await _saleReturnAppService.CreateAsync(new CreateSaleReturnDto
        {
            SaleOrderId = sale.Id,
            Items = { new CreateSaleReturnItemDto { SaleOrderItemId = itemId, Quantity = 1 } }
        });
        var second = await _saleReturnAppService.CreateAsync(new CreateSaleReturnDto
        {
            SaleOrderId = sale.Id,
            Items = { new CreateSaleReturnItemDto { SaleOrderItemId = itemId, Quantity = 2 } }
        });

        first.RefundAmount.ShouldBe(9.90m);
        (first.RefundAmount + second.RefundAmount).ShouldBe(sale.TotalAmount);
        (await GetQuantityOnHandAsync(productId)).ShouldBe(10);
    }

    [Fact]
    public async Task Returning_More_Than_Sold_Is_Rejected()
    {
        var productId = await CreateProductAsync();
        var sale = await _saleOrderAppService.CreateAsync(SaleOf(productId, 2));

        await Should.ThrowAsync<UserFriendlyException>(() => _saleReturnAppService.CreateAsync(new CreateSaleReturnDto
        {
            SaleOrderId = sale.Id,
            Items = { new CreateSaleReturnItemDto { SaleOrderItemId = sale.Items.Single().Id, Quantity = 3 } }
        }));
    }

    [Fact]
    public async Task Concurrent_Stock_Updates_Are_Detected_By_Concurrency_Stamp()
    {
        var productId = await CreateProductAsync(quantity: 5);

        // Two clerks load the same product and both sell the last units; the second save must fail.
        await Should.ThrowAsync<AbpDbConcurrencyException>(async () =>
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var stale = await _productRepository.GetAsync(productId);

                // requiresNew gives the second clerk their own DbContext instead of sharing the tracked entity.
                using (var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true))
                {
                    var other = await _productRepository.GetAsync(productId);
                    other.QuantityOnHand -= 5;
                    await _productRepository.UpdateAsync(other, autoSave: true);
                    await uow.CompleteAsync();
                }

                stale.QuantityOnHand -= 5;
                await _productRepository.UpdateAsync(stale, autoSave: true);
            });
        });

        (await GetQuantityOnHandAsync(productId)).ShouldBe(0);
    }
}
