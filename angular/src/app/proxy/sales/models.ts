import type { DiscountType } from './discount-type.enum';
import type { PaymentMethod } from './payment-method.enum';
import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateSaleOrderDto {
  items: CreateSaleOrderItemDto[];
  discountType?: DiscountType;
  discountValue: number;
  paymentMethod?: PaymentMethod;
  amountTendered?: number;
}

export interface CreateSaleOrderItemDto {
  productId: string;
  quantity: number;
}

export interface CreateSaleReturnDto {
  saleOrderId: string;
  reason?: string;
  items: CreateSaleReturnItemDto[];
}

export interface CreateSaleReturnItemDto {
  saleOrderItemId: string;
  quantity: number;
}

export interface GetSaleReturnListDto extends PagedAndSortedResultRequestDto {
  saleOrderId?: string;
}

export interface PricingInfoDto {
  taxRatePercent: number;
}

export interface SaleOrderDto extends AuditedEntityDto<string> {
  subtotal: number;
  discountType?: DiscountType;
  discountValue: number;
  discountAmount: number;
  taxRatePercent: number;
  taxAmount: number;
  totalAmount: number;
  paymentMethod?: PaymentMethod;
  amountTendered: number;
  changeGiven: number;
  refundedAmount: number;
  items: SaleOrderItemDto[];
}

export interface SaleOrderItemDto {
  id?: string;
  productId?: string;
  quantity: number;
  unitPrice: number;
  returnedQuantity: number;
}

export interface SaleReturnDto extends AuditedEntityDto<string> {
  saleOrderId?: string;
  reason?: string;
  refundAmount: number;
  items: SaleReturnItemDto[];
}

export interface SaleReturnItemDto {
  saleOrderItemId?: string;
  productId?: string;
  quantity: number;
  refundAmount: number;
}
