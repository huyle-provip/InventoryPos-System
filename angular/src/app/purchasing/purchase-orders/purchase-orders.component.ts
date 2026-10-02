import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { PurchaseOrderService } from '../../proxy/purchasing/purchase-order.service';
import { SupplierService } from '../../proxy/purchasing/supplier.service';
import { CreatePurchaseOrderItemDto, PurchaseOrderDto, SupplierDto } from '../../proxy/purchasing/models';
import { PurchaseOrderStatus } from '../../proxy/purchasing/purchase-order-status.enum';
import { ProductService } from '../../proxy/catalog/product.service';
import { ProductDto } from '../../proxy/catalog/models';

@Component({
  standalone: false,
  selector: 'app-purchase-orders',
  templateUrl: './purchase-orders.component.html',
  providers: [ListService],
})
export class PurchaseOrdersComponent implements OnInit {
  readonly list = inject(ListService);

  PurchaseOrderStatus = PurchaseOrderStatus;

  orders: PagedResultDto<PurchaseOrderDto> = { items: [], totalCount: 0 };
  suppliers: SupplierDto[] = [];
  products: ProductDto[] = [];
  expandedId: string | null = null;

  isModalOpen = false;
  isSaving = false;
  supplierId = '';
  note = '';
  lines: CreatePurchaseOrderItemDto[] = [];

  constructor(
    private purchaseOrderService: PurchaseOrderService,
    private supplierService: SupplierService,
    private productService: ProductService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {}

  ngOnInit() {
    this.list.maxResultCount = 100;
    this.supplierService.getList({ maxResultCount: 1000 }).subscribe(r => (this.suppliers = r.items));
    this.productService.getList({ maxResultCount: 1000 }).subscribe(r => (this.products = r.items));

    this.list
      .hookToQuery((query: any) => this.purchaseOrderService.getList(query))
      .subscribe(result => {
        this.orders = result;
      });
  }

  statusLabel(status?: PurchaseOrderStatus): string {
    switch (status) {
      case PurchaseOrderStatus.Open:
        return 'Open';
      case PurchaseOrderStatus.PartiallyReceived:
        return 'Partially received';
      case PurchaseOrderStatus.Received:
        return 'Received';
      case PurchaseOrderStatus.Cancelled:
        return 'Cancelled';
      default:
        return '-';
    }
  }

  statusClass(status?: PurchaseOrderStatus): string {
    switch (status) {
      case PurchaseOrderStatus.Open:
        return 'bg-primary';
      case PurchaseOrderStatus.PartiallyReceived:
        return 'bg-warning text-dark';
      case PurchaseOrderStatus.Received:
        return 'bg-success';
      default:
        return 'bg-secondary';
    }
  }

  toggle(order: PurchaseOrderDto) {
    this.expandedId = this.expandedId === order.id ? null : order.id!;
  }

  receivedSummary(order: PurchaseOrderDto): string {
    const ordered = order.items.reduce((sum, i) => sum + i.quantityOrdered, 0);
    const received = order.items.reduce((sum, i) => sum + i.quantityReceived, 0);
    return `${received} / ${ordered}`;
  }

  openCreate() {
    this.supplierId = '';
    this.note = '';
    this.lines = [{ productId: '', quantity: 1, unitCost: 0 }];
    this.isModalOpen = true;
  }

  addLine() {
    this.lines.push({ productId: '', quantity: 1, unitCost: 0 });
  }

  removeLine(index: number) {
    this.lines.splice(index, 1);
  }

  get total(): number {
    return this.lines.reduce((sum, l) => sum + (l.quantity || 0) * (l.unitCost || 0), 0);
  }

  get canSave(): boolean {
    return (
      !!this.supplierId &&
      this.lines.length > 0 &&
      this.lines.every(l => !!l.productId && l.quantity >= 1 && l.unitCost >= 0)
    );
  }

  save() {
    if (!this.canSave) {
      return;
    }

    this.isSaving = true;
    this.purchaseOrderService
      .create({ supplierId: this.supplierId, note: this.note || undefined, items: this.lines })
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe(order => {
        this.isModalOpen = false;
        this.toaster.success(`Purchase order ${order.orderNumber} created`);
        this.list.get();
      });
  }

  cancel(order: PurchaseOrderDto) {
    this.confirmation
      .warn(`Cancel purchase order ${order.orderNumber}?`, 'Are you sure?')
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.purchaseOrderService.cancel(order.id!).subscribe(() => {
            this.toaster.success('Purchase order cancelled');
            this.list.get();
          });
        }
      });
  }
}
