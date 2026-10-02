using System;
using System.Collections.Generic;

namespace InventoryPos.Dashboard;

public class DashboardDto
{
    public decimal TodaySalesTotal { get; set; }

    public int TodayOrderCount { get; set; }

    public int TotalProducts { get; set; }

    public int LowStockCount { get; set; }

    public decimal InventoryValue { get; set; }

    public List<DailySalesDto> SalesLast7Days { get; set; } = new();

    public List<TopProductDto> TopProducts { get; set; } = new();

    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
}

public class DailySalesDto
{
    public DateTime Date { get; set; }

    public decimal Total { get; set; }

    public int OrderCount { get; set; }
}

public class TopProductDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int QuantitySold { get; set; }

    public decimal Revenue { get; set; }
}

public class LowStockProductDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int QuantityOnHand { get; set; }

    public int ReorderThreshold { get; set; }
}
