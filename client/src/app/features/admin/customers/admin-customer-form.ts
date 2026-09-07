import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomerAdminService } from '../../../core/services/customer-admin.service';
import { PhoneMaskDirective } from '../../../shared/directives/phone-mask.directive';

@Component({
  selector: 'app-admin-customer-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, PhoneMaskDirective],
  templateUrl: './admin-customer-form.html',
})
export class AdminCustomerForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly customerAdminService = inject(CustomerAdminService);

  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private customerId!: string;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    cpf: ['', [Validators.required, Validators.pattern(/^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$/)]],
    phone: ['', Validators.required],
  });

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('id')!;

    this.customerAdminService.getById(this.customerId).subscribe({
      next: (customer) => {
        this.form.patchValue({
          name: customer.name,
          email: customer.email,
          cpf: customer.cpf ?? '',
          phone: customer.phone ?? '',
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
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

    this.customerAdminService
      .update(this.customerId, { name: value.name, email: value.email, cpf: value.cpf, phone: value.phone || null })
      .subscribe({
        next: () => this.router.navigate(['/admin/clientes']),
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(err?.error?.detail ?? 'Não foi possível salvar os dados do cliente.');
        },
      });
  }
}
