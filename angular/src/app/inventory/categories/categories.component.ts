import { Component, OnInit, inject } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { CategoryService } from '../../proxy/catalog/category.service';
import { CategoryDto, CreateUpdateCategoryDto } from '../../proxy/catalog/models';

@Component({
  standalone: false,
  selector: 'app-categories',
  templateUrl: './categories.component.html',
  providers: [ListService],
})
export class CategoriesComponent implements OnInit {
  readonly list = inject(ListService);

  categories: PagedResultDto<CategoryDto> = { items: [], totalCount: 0 };

  isModalOpen = false;
  isSaving = false;
  editingId: string | null = null;
  form: CreateUpdateCategoryDto = { name: '' };

  constructor(
    private categoryService: CategoryService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {}

  ngOnInit() {
    const categoryStreamCreator = (query: any) => this.categoryService.getList(query);

    this.list.hookToQuery(categoryStreamCreator).subscribe(result => {
      this.categories = result;
    });
  }

  openCreate() {
    this.editingId = null;
    this.form = { name: '' };
    this.isModalOpen = true;
  }

  openEdit(category: CategoryDto) {
    this.editingId = category.id!;
    this.form = { name: category.name ?? '' };
    this.isModalOpen = true;
  }

  save() {
    this.isSaving = true;
    const request = this.editingId
      ? this.categoryService.update(this.editingId, this.form)
      : this.categoryService.create(this.form);

    request.pipe(finalize(() => (this.isSaving = false))).subscribe(() => {
      this.isModalOpen = false;
      this.toaster.success('Saved successfully');
      this.list.get();
    });
  }

  delete(category: CategoryDto) {
    this.confirmation
      .warn('Are you sure you want to delete this category?', 'Are you sure?')
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.categoryService.delete(category.id!).subscribe(() => {
            this.toaster.success('Deleted successfully');
            this.list.get();
          });
        }
      });
  }
}
