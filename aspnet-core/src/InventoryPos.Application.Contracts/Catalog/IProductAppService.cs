using System;
using Volo.Abp.Application.Services;

namespace InventoryPos.Catalog;

public interface IProductAppService :
    ICrudAppService<ProductDto, Guid, GetProductListDto, CreateUpdateProductDto>
{
}
