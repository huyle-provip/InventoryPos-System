import { mapEnumToOptions } from '@abp/ng.core';

export enum PaymentMethod {
  Cash = 1,
  Card = 2,
}

export const paymentMethodOptions = mapEnumToOptions(PaymentMethod);
