import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { PurchasingRoutingModule } from './purchasing-routing.module';
import { SuppliersComponent } from './suppliers/suppliers.component';
import { PurchaseOrdersComponent } from './purchase-orders/purchase-orders.component';

@NgModule({
  declarations: [SuppliersComponent, PurchaseOrdersComponent],
  imports: [SharedModule, PurchasingRoutingModule],
})
export class PurchasingModule {}
