import type { CreateStockTransactionDto, GetStockTransactionListDto, StockTransactionDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StockTransactionService {
  apiName = 'Default';
  

  create = (input: CreateStockTransactionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StockTransactionDto>({
      method: 'POST',
      url: '/api/app/stock-transaction',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStockTransactionListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StockTransactionDto>>({
      method: 'GET',
      url: '/api/app/stock-transaction',
      params: { productId: input.productId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
