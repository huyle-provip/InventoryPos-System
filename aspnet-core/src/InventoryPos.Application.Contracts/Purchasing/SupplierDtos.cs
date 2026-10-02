using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InventoryPos.Purchasing;

public class SupplierDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}

public class CreateUpdateSupplierDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [StringLength(128)]
    public string? ContactName { get; set; }

    [StringLength(32)]
    public string? Phone { get; set; }

    [StringLength(256)]
    [EmailAddress]
    public string? Email { get; set; }
}

public interface ISupplierAppService :
    ICrudAppService<SupplierDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateSupplierDto>
{
}
