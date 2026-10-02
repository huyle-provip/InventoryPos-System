import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { SaleOrderService } from '../../proxy/sales/sale-order.service';
import { SaleOrderDto } from '../../proxy/sales/models';
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

  orders: PagedResultDto<SaleOrderDto> = { items: [], totalCount: 0 };
  products: ProductDto[] = [];

  constructor(
    private saleOrderService: SaleOrderService,
    private productService: ProductService,
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
            item.unitPrice,
            item.quantity * item.unitPrice,
          ]);
        }
      }

      downloadCsv(
        `sales-${csvTimestamp()}.csv`,
        ['Date', 'Order Id', 'Product', 'Quantity', 'Unit Price', 'Line Total'],
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
}
