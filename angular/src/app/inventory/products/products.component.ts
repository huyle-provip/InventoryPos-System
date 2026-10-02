import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { ProductService } from '../../proxy/catalog/product.service';
import { CategoryService } from '../../proxy/catalog/category.service';
import { CreateUpdateProductDto, ProductDto, GetProductListDto } from '../../proxy/catalog/models';
import { CategoryDto } from '../../proxy/catalog/models';
import { csvTimestamp, downloadCsv } from '../../shared/csv.util';

@Component({
  standalone: false,
  selector: 'app-products',
  templateUrl: './products.component.html',
  providers: [ListService],
})
export class ProductsComponent implements OnInit {
  readonly list = inject(ListService);

  products: PagedResultDto<ProductDto> = { items: [], totalCount: 0 };
  categories: CategoryDto[] = [];

  filterText = '';
  lowStockOnly = false;

  isModalOpen = false;
  isSaving = false;
  editingId: string | null = null;
  form: CreateUpdateProductDto = { sku: '', name: '', categoryId: undefined, price: 0, reorderThreshold: 0 };

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {}

  exportCsv() {
    this.productService
      .getList({
        maxResultCount: 1000,
        filter: this.filterText,
        lowStockOnly: this.lowStockOnly || undefined,
      })
      .subscribe(result => {
        downloadCsv(
          `products-${csvTimestamp()}.csv`,
          ['SKU', 'Name', 'Category', 'Price', 'On Hand', 'Reorder At', 'Low Stock'],
          result.items.map(p => [
            p.sku,
            p.name,
            this.categoryName(p.categoryId),
            p.price,
            p.quantityOnHand,
            p.reorderThreshold,
            p.isLowStock ? 'Yes' : 'No',
          ]),
        );
      });
  }

  ngOnInit() {
    this.list.maxResultCount = 100;
    this.categoryService.getList({ maxResultCount: 1000 }).subscribe(result => (this.categories = result.items));

    const productStreamCreator = (query: any) =>
      this.productService.getList({
        ...query,
        filter: this.filterText,
        lowStockOnly: this.lowStockOnly || undefined,
      } as GetProductListDto);

    this.list.hookToQuery(productStreamCreator).subscribe(result => {
      this.products = result;
    });
  }

  applyFilters() {
    this.list.get();
  }

  categoryName(categoryId?: string): string {
    if (!categoryId) {
      return '-';
    }
    return this.categories.find(c => c.id === categoryId)?.name ?? '-';
  }

  openCreate() {
    this.editingId = null;
    this.form = { sku: '', name: '', categoryId: undefined, price: 0, reorderThreshold: 0 };
    this.isModalOpen = true;
  }

  openEdit(product: ProductDto) {
    this.editingId = product.id!;
    this.form = {
      sku: product.sku ?? '',
      name: product.name ?? '',
      categoryId: product.categoryId,
      price: product.price,
      reorderThreshold: product.reorderThreshold,
    };
    this.isModalOpen = true;
  }

  save() {
    this.isSaving = true;
    const request = this.editingId
      ? this.productService.update(this.editingId, this.form)
      : this.productService.create(this.form);

    request.pipe(finalize(() => (this.isSaving = false))).subscribe(() => {
      this.isModalOpen = false;
      this.toaster.success('Saved successfully');
      this.list.get();
    });
  }

  delete(product: ProductDto) {
    this.confirmation
      .warn('Are you sure you want to delete this product?', 'Are you sure?')
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.productService.delete(product.id!).subscribe(() => {
            this.toaster.success('Deleted successfully');
            this.list.get();
          });
        }
      });
  }
}
