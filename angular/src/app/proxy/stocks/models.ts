import type { StockTransactionType } from './stock-transaction-type.enum';
import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateStockTransactionDto {
  productId: string;
  type: StockTransactionType;
  quantity: number;
  note?: string;
}

export interface GetStockTransactionListDto extends PagedAndSortedResultRequestDto {
  productId?: string;
}

export interface StockTransactionDto extends AuditedEntityDto<string> {
  productId?: string;
  type?: StockTransactionType;
  quantity: number;
  note?: string;
}
