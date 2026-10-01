using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Catalog;

public class Category : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
}
