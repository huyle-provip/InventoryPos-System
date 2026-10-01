import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CategoryDto extends AuditedEntityDto<string> {
  name?: string;
}

export interface CreateUpdateCategoryDto {
  name: string;
}

export interface CreateUpdateProductDto {
  sku: string;
  name: string;
  categoryId?: string;
  price: number;
  reorderThreshold: number;
}

export interface GetProductListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  categoryId?: string;
  lowStockOnly?: boolean;
}

export interface ProductDto extends AuditedEntityDto<string> {
  sku?: string;
  name?: string;
  categoryId?: string;
  price: number;
  quantityOnHand: number;
  reorderThreshold: number;
  isLowStock: boolean;
}
