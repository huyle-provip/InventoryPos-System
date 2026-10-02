import type { CreateSaleReturnDto, GetSaleReturnListDto, SaleReturnDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SaleReturnService {
  apiName = 'Default';
  

  create = (input: CreateSaleReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SaleReturnDto>({
      method: 'POST',
      url: '/api/app/sale-return',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSaleReturnListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SaleReturnDto>>({
      method: 'GET',
      url: '/api/app/sale-return',
      params: { saleOrderId: input.saleOrderId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
