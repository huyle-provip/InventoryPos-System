using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace InventoryPos.Sales;

public class SaleReturn : AuditedAggregateRoot<Guid>
{
    public Guid SaleOrderId { get; set; }

    public string? Reason { get; set; }

    public decimal RefundAmount { get; set; }

    public List<SaleReturnItem> Items { get; set; } = new();
}
