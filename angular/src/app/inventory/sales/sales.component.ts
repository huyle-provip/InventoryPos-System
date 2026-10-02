import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { SaleOrderService } from '../../proxy/sales/sale-order.service';
import { SaleReturnService } from '../../proxy/sales/sale-return.service';
import { SaleOrderDto, SaleOrderItemDto } from '../../proxy/sales/models';
import { DiscountType } from '../../proxy/sales/discount-type.enum';
import { PaymentMethod } from '../../proxy/sales/payment-method.enum';
import { ProductService } from '../../proxy/catalog/product.service';
import { ProductDto } from '../../proxy/catalog/models';
import { CsvCell, csvTimestamp, downloadCsv } from '../../shared/csv.util';

@Component({
  standalone: false,
  selector: 'app-sales',
  templateUrl: './sales.component.html',
  providers: [ListService],
})
export class SalesComponent implements OnInit {
  readonly list = inject(ListService);

  DiscountType = DiscountType;
  PaymentMethod = PaymentMethod;

  orders: PagedResultDto<SaleOrderDto> = { items: [], totalCount: 0 };
  products: ProductDto[] = [];

  isRefundOpen = false;
  isRefunding = false;
  refundOrder: SaleOrderDto | null = null;
  refundQuantities: Record<string, number> = {};
  refundReason = '';

  constructor(
    private saleOrderService: SaleOrderService,
    private saleReturnService: SaleReturnService,
    private productService: ProductService,
    private toaster: ToasterService,
  ) {}

  exportCsv() {
    this.saleOrderService.getList({ maxResultCount: 1000 }).subscribe(result => {
      const rows: CsvCell[][] = [];
      for (const order of result.items) {
        for (const item of order.items) {
          rows.push([
            order.creationTime,
            order.id,
            this.productName(item.productId),
            item.quantity,
            item.returnedQuantity,
            item.unitPrice,
            item.quantity * item.unitPrice,
            order.discountAmount,
            order.taxAmount,
            order.totalAmount,
            order.paymentMethod === PaymentMethod.Card ? 'Card' : 'Cash',
            order.refundedAmount,
          ]);
        }
      }

      downloadCsv(
        `sales-${csvTimestamp()}.csv`,
        [
          'Date', 'Order Id', 'Product', 'Quantity', 'Returned', 'Unit Price', 'Line Total',
          'Order Discount', 'Order Tax', 'Order Total', 'Payment', 'Order Refunded',
        ],
        rows,
      );
    });
  }

  ngOnInit() {
    this.list.maxResultCount = 100;
    this.productService.getList({ maxResultCount: 1000 }).subscribe(result => (this.products = result.items));

    const orderStreamCreator = (query: any) => this.saleOrderService.getList(query);

    this.list.hookToQuery(orderStreamCreator).subscribe(result => {
      this.orders = result;
    });
  }

  productName(productId?: string): string {
    if (!productId) {
      return '-';
    }
    return this.products.find(p => p.id === productId)?.name ?? '-';
  }

  discountLabel(order: SaleOrderDto): string {
    if (order.discountType === DiscountType.Percent) {
      return `Discount (${order.discountValue}%)`;
    }
    return 'Discount';
  }

  returnable(item: SaleOrderItemDto): number {
    return item.quantity - item.returnedQuantity;
  }

  canRefund(order: SaleOrderDto): boolean {
    return order.items.some(i => this.returnable(i) > 0);
  }

  openRefund(order: SaleOrderDto) {
    this.refundOrder = order;
    this.refundQuantities = {};
    this.refundReason = '';
    this.isRefundOpen = true;
  }

  /** Preview only; the server computes the authoritative amount (each unit gets its share of the sale's discount and tax). */
  get estimatedRefund(): number {
    if (!this.refundOrder) {
      return 0;
    }
    const itemsSubtotal = this.refundOrder.items.reduce((sum, i) => sum + i.quantity * i.unitPrice, 0);
    const ratio = itemsSubtotal > 0 ? this.refundOrder.totalAmount / itemsSubtotal : 1;
    return this.refundOrder.items.reduce(
      (sum, i) => sum + (this.refundQuantities[i.id!] || 0) * i.unitPrice * ratio,
      0,
    );
  }

  get hasRefundSelection(): boolean {
    return Object.values(this.refundQuantities).some(q => q > 0);
  }

  submitRefund() {
    if (!this.refundOrder || !this.hasRefundSelection) {
      return;
    }

    const items = this.refundOrder.items
      .filter(i => (this.refundQuantities[i.id!] || 0) > 0)
      .map(i => ({ saleOrderItemId: i.id!, quantity: this.refundQuantities[i.id!] }));

    this.isRefunding = true;
    this.saleReturnService
      .create({ saleOrderId: this.refundOrder.id!, reason: this.refundReason || undefined, items })
      .pipe(finalize(() => (this.isRefunding = false)))
      .subscribe(result => {
        this.isRefundOpen = false;
        this.toaster.success(`Refunded ${result.refundAmount.toFixed(2)} and returned the items to stock.`);
        this.list.get();
      });
  }
}
