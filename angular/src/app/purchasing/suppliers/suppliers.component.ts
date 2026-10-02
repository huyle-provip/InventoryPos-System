import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { SupplierService } from '../../proxy/purchasing/supplier.service';
import { CreateUpdateSupplierDto, SupplierDto } from '../../proxy/purchasing/models';

@Component({
  standalone: false,
  selector: 'app-suppliers',
  templateUrl: './suppliers.component.html',
  providers: [ListService],
})
export class SuppliersComponent implements OnInit {
  readonly list = inject(ListService);

  suppliers: PagedResultDto<SupplierDto> = { items: [], totalCount: 0 };

  isModalOpen = false;
  isSaving = false;
  editingId: string | null = null;
  form: CreateUpdateSupplierDto = { name: '' };

  constructor(
    private supplierService: SupplierService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {}

  ngOnInit() {
    this.list.maxResultCount = 100;
    this.list.hookToQuery((query: any) => this.supplierService.getList(query)).subscribe(result => {
      this.suppliers = result;
    });
  }

  openCreate() {
    this.editingId = null;
    this.form = { name: '' };
    this.isModalOpen = true;
  }

  openEdit(supplier: SupplierDto) {
    this.editingId = supplier.id!;
    this.form = {
      name: supplier.name ?? '',
      contactName: supplier.contactName,
      phone: supplier.phone,
      email: supplier.email,
    };
    this.isModalOpen = true;
  }

  save() {
    this.isSaving = true;
    const request = this.editingId
      ? this.supplierService.update(this.editingId, this.form)
      : this.supplierService.create(this.form);

    request.pipe(finalize(() => (this.isSaving = false))).subscribe(() => {
      this.isModalOpen = false;
      this.toaster.success('Saved successfully');
      this.list.get();
    });
  }

  delete(supplier: SupplierDto) {
    this.confirmation
      .warn('Delete this supplier? Suppliers with purchase orders cannot be deleted.', 'Are you sure?')
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.supplierService.delete(supplier.id!).subscribe(() => {
            this.toaster.success('Deleted successfully');
            this.list.get();
          });
        }
      });
  }
}
