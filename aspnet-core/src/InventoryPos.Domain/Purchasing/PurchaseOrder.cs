using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Purchasing;

public class PurchaseOrder : AuditedAggregateRoot<Guid>
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Open;

    public string? Note { get; set; }

    public decimal TotalCost { get; set; }

    public List<PurchaseOrderItem> Items { get; set; } = new();

    public void RefreshStatusFromItems()
    {
        if (Items.All(i => i.QuantityReceived >= i.QuantityOrdered))
        {
            Status = PurchaseOrderStatus.Received;
        }
        else if (Items.Any(i => i.QuantityReceived > 0))
        {
            Status = PurchaseOrderStatus.PartiallyReceived;
        }
        else
        {
            Status = PurchaseOrderStatus.Open;
        }
    }
}
