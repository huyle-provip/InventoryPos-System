import type { CreateSaleOrderDto, PricingInfoDto, SaleOrderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SaleOrderService {
  apiName = 'Default';
  

  create = (input: CreateSaleOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleOrderDto>({
      method: 'POST',
      url: '/api/app/sale-order',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleOrderDto>({
      method: 'GET',
      url: `/api/app/sale-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SaleOrderDto>>({
      method: 'GET',
      url: '/api/app/sale-order',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getPricingInfo = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PricingInfoDto>({
      method: 'GET',
      url: '/api/app/sale-order/pricing-info',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
