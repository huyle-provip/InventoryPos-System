using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace InventoryPos.Purchasing;

public class PurchaseOrderItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int QuantityOrdered { get; set; }

    public int QuantityReceived { get; set; }

    public int QuantityOutstanding => Math.Max(0, QuantityOrdered - QuantityReceived);

    public decimal UnitCost { get; set; }
}

public class PurchaseOrderDto : AuditedEntityDto<Guid>
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public PurchaseOrderStatus Status { get; set; }

    public string? Note { get; set; }

    public decimal TotalCost { get; set; }

    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitCost { get; set; }
}

public class CreatePurchaseOrderDto
{
    [Required]
    public Guid SupplierId { get; set; }

    [StringLength(512)]
    public string? Note { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class ReceivePurchaseOrderItemDto
{
    [Required]
    public Guid PurchaseOrderItemId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}

public class ReceivePurchaseOrderDto
{
    [Required]
    [MinLength(1)]
    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = new();
}

public class GetPurchaseOrderListDto : PagedAndSortedResultRequestDto
{
    public PurchaseOrderStatus? Status { get; set; }

    /// <summary>When true, only orders that can still be received (Open or PartiallyReceived).</summary>
    public bool? ReceivableOnly { get; set; }
}

public interface IPurchaseOrderAppService : IApplicationService
{
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input);

    Task<PurchaseOrderDto> GetAsync(Guid id);

    Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input);

    Task<PurchaseOrderDto> ReceiveAsync(Guid id, ReceivePurchaseOrderDto input);

    Task<PurchaseOrderDto> CancelAsync(Guid id);
}
