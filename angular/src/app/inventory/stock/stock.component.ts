import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { StockTransactionService } from '../../proxy/stocks/stock-transaction.service';
import { CreateStockTransactionDto, StockTransactionDto } from '../../proxy/stocks/models';
import { StockTransactionType } from '../../proxy/stocks/stock-transaction-type.enum';
import { ProductService } from '../../proxy/catalog/product.service';
import { ProductDto } from '../../proxy/catalog/models';
import { csvTimestamp, downloadCsv } from '../../shared/csv.util';

@Component({
  standalone: false,
  selector: 'app-stock',
  templateUrl: './stock.component.html',
  providers: [ListService],
})
export class StockComponent implements OnInit {
  readonly list = inject(ListService);

  StockTransactionType = StockTransactionType;

  transactions: PagedResultDto<StockTransactionDto> = { items: [], totalCount: 0 };
  products: ProductDto[] = [];

  isSaving = false;
  form: CreateStockTransactionDto = {
    productId: '',
    type: StockTransactionType.In,
    quantity: 1,
    note: '',
  };

  constructor(
    private stockTransactionService: StockTransactionService,
    private productService: ProductService,
    private toaster: ToasterService,
  ) {}

  exportCsv() {
    this.stockTransactionService.getList({ maxResultCount: 1000 }).subscribe(result => {
      downloadCsv(
        `stock-transactions-${csvTimestamp()}.csv`,
        ['Date', 'Product', 'Type', 'Quantity', 'Note'],
        result.items.map(tx => [
          tx.creationTime,
          this.productName(tx.productId),
          tx.type === StockTransactionType.In ? 'In' : 'Out',
          tx.quantity,
          tx.note,
        ]),
      );
    });
  }

  ngOnInit() {
    this.list.maxResultCount = 100;
    this.loadProducts();

    const transactionStreamCreator = (query: any) => this.stockTransactionService.getList(query);

    this.list.hookToQuery(transactionStreamCreator).subscribe(result => {
      this.transactions = result;
    });
  }

  loadProducts() {
    this.productService.getList({ maxResultCount: 1000 }).subscribe(result => (this.products = result.items));
  }

  productName(productId?: string): string {
    if (!productId) {
      return '-';
    }
    return this.products.find(p => p.id === productId)?.name ?? '-';
  }

  submit() {
    if (!this.form.productId || this.form.quantity < 1) {
      return;
    }

    this.isSaving = true;
    this.stockTransactionService
      .create(this.form)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe(() => {
        this.toaster.success('Stock transaction recorded');
        this.form = { productId: '', type: StockTransactionType.In, quantity: 1, note: '' };
        this.loadProducts();
        this.list.get();
      });
  }
}
