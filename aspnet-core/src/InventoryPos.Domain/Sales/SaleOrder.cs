using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Sales;

public class SaleOrder : AuditedAggregateRoot<Guid>
{
    public decimal TotalAmount { get; set; }

    public List<SaleOrderItem> Items { get; set; } = new();
}
