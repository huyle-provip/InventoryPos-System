import { mapEnumToOptions } from '@abp/ng.core';

export enum DiscountType {
  None = 0,
  Percent = 1,
  Amount = 2,
}

export const discountTypeOptions = mapEnumToOptions(DiscountType);
