import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CurrencyPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.html',
})
export class Checkout implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    customerName: ['', Validators.required],
    customerEmail: ['', [Validators.required, Validators.email]],
    customerPhone: [''],
    street: ['', Validators.required],
    number: ['', Validators.required],
    complement: [''],
    neighborhood: ['', Validators.required],
    city: ['', Validators.required],
    state: ['', Validators.required],
    zipCode: ['', Validators.required],
    notes: [''],
  });

  constructor(
    readonly cart: CartService,
    private readonly orderService: OrderService,
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    if (this.cart.items().length === 0) {
      this.router.navigate(['/carrinho']);
      return;
    }

    const user = this.auth.currentUser();
    if (user) {
      this.form.patchValue({ customerName: user.name, customerEmail: user.email });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);

    const shippingAddress = {
      street: value.street,
      number: value.number,
      complement: value.complement || null,
      neighborhood: value.neighborhood,
      city: value.city,
      state: value.state,
      zipCode: value.zipCode,
    };

    this.orderService
      .createStoreOrder({
        customerName: value.customerName,
        customerEmail: value.customerEmail,
        customerPhone: value.customerPhone || null,
        notes: value.notes || null,
        shippingAddressJson: JSON.stringify(shippingAddress),
        items: this.cart.items().map((item) => ({
          productId: item.product.id,
          productName: item.product.name,
          unitPrice: item.product.price,
          quantity: item.quantity,
          optionsJson: null,
        })),
      })
      .subscribe({
        next: (order) => {
          this.cart.clear();
          this.router.navigate(['/pedido', order.id]);
        },
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(err?.error?.detail ?? 'Não foi possível finalizar o pedido. Tente novamente.');
        },
      });
  }
}
