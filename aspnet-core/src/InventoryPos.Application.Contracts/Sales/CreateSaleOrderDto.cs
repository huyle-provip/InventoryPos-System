using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryPos.Sales;

public class CreateSaleOrderDto
{
    [Required]
    [MinLength(1)]
    public List<CreateSaleOrderItemDto> Items { get; set; } = new();
}
