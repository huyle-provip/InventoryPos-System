using System;
using InventoryPos.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace InventoryPos.Purchasing;

public class SupplierAppService :
    CrudAppService<Supplier, SupplierDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateSupplierDto>,
    ISupplierAppService
{
    public SupplierAppService(IRepository<Supplier, Guid> repository)
        : base(repository)
    {
        GetPolicyName = InventoryPosPermissions.Suppliers.Default;
        GetListPolicyName = InventoryPosPermissions.Suppliers.Default;
        CreatePolicyName = InventoryPosPermissions.Suppliers.Create;
        UpdatePolicyName = InventoryPosPermissions.Suppliers.Edit;
        DeletePolicyName = InventoryPosPermissions.Suppliers.Delete;
    }
}
