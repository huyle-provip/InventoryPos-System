import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '@abp/ng.core';
import { policyGuard } from '../shared/policy.guard';
import { CategoriesComponent } from './categories/categories.component';
import { ProductsComponent } from './products/products.component';
import { StockComponent } from './stock/stock.component';
import { SalesComponent } from './sales/sales.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  {
    path: 'categories',
    component: CategoriesComponent,
    canActivate: [authGuard, policyGuard],
    data: { requiredPolicy: 'InventoryPos.Categories' },
  },
  {
    path: 'products',
    component: ProductsComponent,
    canActivate: [authGuard, policyGuard],
    data: { requiredPolicy: 'InventoryPos.Products' },
  },
  {
    path: 'stock',
    component: StockComponent,
    canActivate: [authGuard, policyGuard],
    data: { requiredPolicy: 'InventoryPos.StockTransactions' },
  },
  {
    path: 'sales',
    component: SalesComponent,
    canActivate: [authGuard, policyGuard],
    data: { requiredPolicy: 'InventoryPos.SaleOrders' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InventoryRoutingModule {}
