import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseOrderStatus {
  Open = 1,
  PartiallyReceived = 2,
  Received = 3,
  Cancelled = 4,
}

export const purchaseOrderStatusOptions = mapEnumToOptions(PurchaseOrderStatus);
