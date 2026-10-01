using System;
using InventoryPos.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Catalog;

public class CategoryAppService :
    CrudAppService<Category, CategoryDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCategoryDto>,
    ICategoryAppService
{
    public CategoryAppService(IRepository<Category, Guid> repository)
        : base(repository)
    {
        GetPolicyName = InventoryPosPermissions.Categories.Default;
        GetListPolicyName = InventoryPosPermissions.Categories.Default;
        CreatePolicyName = InventoryPosPermissions.Categories.Create;
        UpdatePolicyName = InventoryPosPermissions.Categories.Edit;
        DeletePolicyName = InventoryPosPermissions.Categories.Delete;
    }
}
