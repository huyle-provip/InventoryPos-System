using System;

namespace InventoryPos.Sales;

public class SaleOrderItemDto
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
