using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace InventoryPos.Sales;

public class CreateSaleReturnItemDto
{
    [Required]
    public Guid SaleOrderItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CreateSaleReturnDto
{
    [Required]
    public Guid SaleOrderId { get; set; }

    [StringLength(512)]
    public string? Reason { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateSaleReturnItemDto> Items { get; set; } = new();
}

public class SaleReturnItemDto
{
    public Guid SaleOrderItemId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal RefundAmount { get; set; }
}

public class SaleReturnDto : AuditedEntityDto<Guid>
{
    public Guid SaleOrderId { get; set; }

    public string? Reason { get; set; }

    public decimal RefundAmount { get; set; }

    public List<SaleReturnItemDto> Items { get; set; } = new();
}

public class GetSaleReturnListDto : PagedAndSortedResultRequestDto
{
    public Guid? SaleOrderId { get; set; }
}
