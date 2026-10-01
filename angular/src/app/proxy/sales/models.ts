import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateSaleOrderDto {
  items: CreateSaleOrderItemDto[];
}

export interface CreateSaleOrderItemDto {
  productId: string;
  quantity: number;
}

export interface SaleOrderDto extends AuditedEntityDto<string> {
  totalAmount: number;
  items: SaleOrderItemDto[];
}

export interface SaleOrderItemDto {
  productId?: string;
  quantity: number;
  unitPrice: number;
}
