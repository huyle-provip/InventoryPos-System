using System;

namespace InventoryPos.Sales;

public class SaleOrderItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int ReturnedQuantity { get; set; }
}
