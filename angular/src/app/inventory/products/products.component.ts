import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Observable, of } from 'rxjs';
import { finalize, map, switchMap } from 'rxjs/operators';
import { ProductService } from '../../proxy/catalog/product.service';
import { ProductImageService } from '../../proxy/catalog/product-image.service';
import { CategoryService } from '../../proxy/catalog/category.service';
import { CreateUpdateProductDto, ProductDto, GetProductListDto } from '../../proxy/catalog/models';
import { CategoryDto } from '../../proxy/catalog/models';
import { csvTimestamp, downloadCsv } from '../../shared/csv.util';
import { printLabels } from '../../shared/label-print';
import { environment } from '../../../environments/environment';

const MAX_IMAGE_BYTES = 1024 * 1024;

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

  editingProduct: ProductDto | null = null;
  pendingImage: File | null = null;
  pendingImagePreview: string | null = null;
  removeImage = false;

  constructor(
    private productService: ProductService,
    private productImageService: ProductImageService,
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

  imageUrl(product: ProductDto): string {
    const version = encodeURIComponent(String(product.lastModificationTime ?? product.creationTime ?? ''));
    return `${environment.apis.default.url}/api/app/product-image/${product.id}?v=${version}`;
  }

  printLabel(product: ProductDto) {
    this.printLabelSheet([product]);
  }

  printAllLabels() {
    this.productService
      .getList({ maxResultCount: 1000, filter: this.filterText, lowStockOnly: this.lowStockOnly || undefined })
      .subscribe(result => this.printLabelSheet(result.items));
  }

  private printLabelSheet(items: ProductDto[]) {
    const opened = printLabels(items.map(p => ({ sku: p.sku ?? '', name: p.name ?? '', price: p.price })));
    if (!opened) {
      this.toaster.warn('Could not open the print window. Allow pop-ups for this site and try again.');
    }
  }

  private resetImageState() {
    if (this.pendingImagePreview) {
      URL.revokeObjectURL(this.pendingImagePreview);
    }
    this.pendingImage = null;
    this.pendingImagePreview = null;
    this.removeImage = false;
  }

  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    if (file.type !== 'image/png' && file.type !== 'image/jpeg') {
      this.toaster.error('Only PNG or JPEG images are supported.');
      return;
    }
    if (file.size > MAX_IMAGE_BYTES) {
      this.toaster.error('The image is larger than 1 MB.');
      return;
    }

    this.resetImageState();
    this.pendingImage = file;
    this.pendingImagePreview = URL.createObjectURL(file);
  }

  clearImage() {
    const hadPending = !!this.pendingImage;
    this.resetImageState();
    // Removing a saved photo is only applied when the form is saved.
    this.removeImage = !hadPending && !!this.editingProduct?.hasImage;
  }

  openCreate() {
    this.resetImageState();
    this.editingProduct = null;
    this.editingId = null;
    this.form = { sku: '', name: '', categoryId: undefined, price: 0, reorderThreshold: 0 };
    this.isModalOpen = true;
  }

  openEdit(product: ProductDto) {
    this.resetImageState();
    this.editingProduct = product;
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

  private applyImageChange(productId: string): Observable<unknown> {
    if (this.pendingImage) {
      const formData = new FormData();
      formData.append('file', this.pendingImage);
      return this.productImageService.update(productId, formData);
    }
    if (this.removeImage) {
      return this.productImageService.delete(productId);
    }
    return of(null);
  }

  save() {
    this.isSaving = true;
    const request = this.editingId
      ? this.productService.update(this.editingId, this.form)
      : this.productService.create(this.form);

    request
      .pipe(
        switchMap(product => this.applyImageChange(product.id!).pipe(map(() => product))),
        finalize(() => (this.isSaving = false)),
      )
      .subscribe(() => {
        this.isModalOpen = false;
        this.resetImageState();
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
