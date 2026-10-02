
export interface DailySalesDto {
  date?: string;
  total: number;
  orderCount: number;
}

export interface DashboardDto {
  todaySalesTotal: number;
  todayOrderCount: number;
  todayRefundTotal: number;
  openPurchaseOrderCount: number;
  totalProducts: number;
  lowStockCount: number;
  inventoryValue: number;
  salesLast7Days: DailySalesDto[];
  topProducts: TopProductDto[];
  lowStockProducts: LowStockProductDto[];
}

export interface LowStockProductDto {
  productId?: string;
  sku?: string;
  name?: string;
  quantityOnHand: number;
  reorderThreshold: number;
}

export interface TopProductDto {
  productId?: string;
  productName?: string;
  quantitySold: number;
  revenue: number;
}
