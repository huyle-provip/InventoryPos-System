using System;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Catalog;

public class ProductDto : AuditedEntityDto<Guid>
{
    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }

    public decimal Price { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReorderThreshold { get; set; }

    public bool HasImage { get; set; }

    public bool IsLowStock => QuantityOnHand <= ReorderThreshold;
}
