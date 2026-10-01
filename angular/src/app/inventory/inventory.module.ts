import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { InventoryRoutingModule } from './inventory-routing.module';
import { CategoriesComponent } from './categories/categories.component';
import { ProductsComponent } from './products/products.component';
import { StockComponent } from './stock/stock.component';
import { SalesComponent } from './sales/sales.component';

@NgModule({
  declarations: [CategoriesComponent, ProductsComponent, StockComponent, SalesComponent],
  imports: [SharedModule, InventoryRoutingModule],
})
export class InventoryModule {}
