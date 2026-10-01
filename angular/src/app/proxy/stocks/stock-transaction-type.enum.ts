import { mapEnumToOptions } from '@abp/ng.core';

export enum StockTransactionType {
  In = 1,
  Out = 2,
}

export const stockTransactionTypeOptions = mapEnumToOptions(StockTransactionType);
