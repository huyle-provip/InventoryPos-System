using System;
using Volo.Abp.Domain.Entities;

namespace InventoryPos.Sales;

public class SaleOrderItem : Entity<Guid>
{
    public Guid SaleOrderId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int ReturnedQuantity { get; set; }
}
