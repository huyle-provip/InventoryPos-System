import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '@abp/ng.core';
import { policyGuard } from '../shared/policy.guard';
import { SuppliersComponent } from './suppliers/suppliers.component';
import { PurchaseOrdersComponent } from './purchase-orders/purchase-orders.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'purchase-orders' },
  {
    path: 'suppliers',
    component: SuppliersComponent,
    canActivate: [authGuard, policyGuard],
    data: { requiredPolicy: 'InventoryPos.Suppliers' },
  },
  {
    path: 'purchase-orders',
    component: PurchaseOrdersComponent,
    canActivate: [authGuard, policyGuard],
    data: { requiredPolicy: 'InventoryPos.PurchaseOrders' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PurchasingRoutingModule {}
