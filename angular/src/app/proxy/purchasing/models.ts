import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PurchaseOrderStatus } from './purchase-order-status.enum';

export interface CreatePurchaseOrderDto {
  supplierId: string;
  note?: string;
  items: CreatePurchaseOrderItemDto[];
}

export interface CreatePurchaseOrderItemDto {
  productId: string;
  quantity: number;
  unitCost: number;
}

export interface CreateUpdateSupplierDto {
  name: string;
  contactName?: string;
  phone?: string;
  email?: string;
}

export interface GetPurchaseOrderListDto extends PagedAndSortedResultRequestDto {
  status?: PurchaseOrderStatus;
  receivableOnly?: boolean;
}

export interface PurchaseOrderDto extends AuditedEntityDto<string> {
  orderNumber?: string;
  supplierId?: string;
  supplierName?: string;
  status?: PurchaseOrderStatus;
  note?: string;
  totalCost: number;
  items: PurchaseOrderItemDto[];
}

export interface PurchaseOrderItemDto {
  id?: string;
  productId?: string;
  productName?: string;
  sku?: string;
  quantityOrdered: number;
  quantityReceived: number;
  quantityOutstanding: number;
  unitCost: number;
}

export interface ReceivePurchaseOrderDto {
  items: ReceivePurchaseOrderItemDto[];
}

export interface ReceivePurchaseOrderItemDto {
  purchaseOrderItemId: string;
  quantity: number;
}

export interface SupplierDto extends AuditedEntityDto<string> {
  name?: string;
  contactName?: string;
  phone?: string;
  email?: string;
}
