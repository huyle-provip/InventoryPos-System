using System;
using Volo.Abp.Domain.Entities;

namespace InventoryPos.Purchasing;

public class PurchaseOrderItem : Entity<Guid>
{
    public Guid PurchaseOrderId { get; set; }

    public Guid ProductId { get; set; }

    public int QuantityOrdered { get; set; }

    public int QuantityReceived { get; set; }

    public decimal UnitCost { get; set; }
}
