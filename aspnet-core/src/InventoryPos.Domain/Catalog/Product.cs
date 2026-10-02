using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Catalog;

public class Product : AuditedAggregateRoot<Guid>
{
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }

    public decimal Price { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReorderThreshold { get; set; }

    public bool HasImage { get; set; }
}
