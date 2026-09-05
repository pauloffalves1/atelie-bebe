import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomerSummary } from '../../../core/models/customer.model';
import { CustomerAdminService } from '../../../core/services/customer-admin.service';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-admin-product-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin-product-form.html',
})
export class AdminProductForm implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly isEditMode = signal(false);
  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly customers = signal<CustomerSummary[]>([]);
  readonly selectedCustomerIds = signal<string[]>([]);
  readonly savingCustomers = signal(false);
  readonly customersSaved = signal(false);

  private productId: string | null = null;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    category: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]],
    imageUrl: [''],
    description: [''],
    featured: [false],
  });

  constructor(
    private readonly productService: ProductService,
    private readonly customerAdminService: CustomerAdminService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.customerAdminService.list().subscribe((customers) => this.customers.set(customers));

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.productId = id;
    this.isEditMode.set(true);
    this.loading.set(true);

    this.productService.getById(id).subscribe({
      next: (product) => {
        this.form.patchValue({
          name: product.name,
          category: product.category,
          price: product.price,
          stock: product.stock,
          imageUrl: product.imageUrl ?? '',
          description: product.description ?? '',
          featured: product.featured,
        });
        this.selectedCustomerIds.set(product.allowedCustomerIds);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggleCustomer(customerId: string, checked: boolean): void {
    const current = this.selectedCustomerIds();
    this.selectedCustomerIds.set(
      checked ? [...current, customerId] : current.filter((id) => id !== customerId),
    );
  }

  saveCustomerAccess(): void {
    if (!this.productId) return;

    this.savingCustomers.set(true);
    this.customersSaved.set(false);
    this.productService.setAllowedCustomers(this.productId, this.selectedCustomerIds()).subscribe({
      next: () => {
        this.savingCustomers.set(false);
        this.customersSaved.set(true);
        setTimeout(() => this.customersSaved.set(false), 2500);
      },
      error: () => this.savingCustomers.set(false),
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);

    const payload = {
      name: value.name,
      description: value.description || null,
      price: value.price,
      category: value.category,
      imageUrl: value.imageUrl || null,
      featured: value.featured,
    };

    const onSuccess = () => this.router.navigate(['/admin/produtos']);
    const onError = (err: any) => {
      this.submitting.set(false);
      this.errorMessage.set(err?.error?.detail ?? 'Não foi possível salvar o produto.');
    };

    if (this.isEditMode() && this.productId) {
      this.productService.update(this.productId, payload).subscribe({
        next: () => this.productService.updateStock(this.productId!, value.stock).subscribe({ next: onSuccess, error: onError }),
        error: onError,
      });
    } else {
      this.productService.create({ ...payload, stock: value.stock }).subscribe({ next: onSuccess, error: onError });
    }
  }
}
