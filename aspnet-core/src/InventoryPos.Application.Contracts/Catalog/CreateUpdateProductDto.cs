using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryPos.Catalog;

public class CreateUpdateProductDto
{
    [Required]
    [StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int ReorderThreshold { get; set; }
}
