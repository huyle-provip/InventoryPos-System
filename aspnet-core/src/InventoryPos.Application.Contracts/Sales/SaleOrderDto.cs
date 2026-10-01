using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Sales;

public class SaleOrderDto : AuditedEntityDto<Guid>
{
    public decimal TotalAmount { get; set; }

    public List<SaleOrderItemDto> Items { get; set; } = new();
}
