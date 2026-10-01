using System;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Catalog;

public class CategoryDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
}
