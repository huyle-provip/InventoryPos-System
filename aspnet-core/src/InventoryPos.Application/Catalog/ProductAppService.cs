using System;
using System.Linq;
using System.Threading.Tasks;
using InventoryPos.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Catalog;

public class ProductAppService :
    CrudAppService<Product, ProductDto, Guid, GetProductListDto, CreateUpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
        GetPolicyName = InventoryPosPermissions.Products.Default;
        GetListPolicyName = InventoryPosPermissions.Products.Default;
        CreatePolicyName = InventoryPosPermissions.Products.Create;
        UpdatePolicyName = InventoryPosPermissions.Products.Edit;
        DeletePolicyName = InventoryPosPermissions.Products.Delete;
    }

    protected override async Task<IQueryable<Product>> CreateFilteredQueryAsync(GetProductListDto input)
    {
        var queryable = await base.CreateFilteredQueryAsync(input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Name.Contains(input.Filter) || x.Sku.Contains(input.Filter));
        }

        if (input.CategoryId.HasValue)
        {
            queryable = queryable.Where(x => x.CategoryId == input.CategoryId);
        }

        if (input.LowStockOnly == true)
        {
            queryable = queryable.Where(x => x.QuantityOnHand <= x.ReorderThreshold);
        }

        return queryable;
    }
}
