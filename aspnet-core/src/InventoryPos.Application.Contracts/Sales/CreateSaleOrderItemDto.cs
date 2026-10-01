using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryPos.Sales;

public class CreateSaleOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
