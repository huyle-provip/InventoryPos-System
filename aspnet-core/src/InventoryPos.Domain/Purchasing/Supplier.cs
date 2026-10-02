using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Purchasing;

public class Supplier : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}
