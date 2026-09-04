import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService } from '../../../core/services/order.service';
import { CustomOrderDetails } from '../../../core/models/order.model';

@Component({
  selector: 'app-custom-order',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './custom-order.html',
})
export class CustomOrder implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly pieceTypes = ['Body', 'Manta', 'Saída de maternidade', 'Kit enxoval', 'Naninha', 'Almofada de amamentação', 'Outro'];
  readonly sizes = ['RN', 'P', 'M', 'G', 'Sob medida'];

  readonly form = this.fb.nonNullable.group({
    customerName: ['', Validators.required],
    customerEmail: ['', [Validators.required, Validators.email]],
    customerPhone: [''],
    tipoPeca: [this.pieceTypes[0], Validators.required],
    tamanho: [this.sizes[0], Validators.required],
    tecido: ['', Validators.required],
    cor: ['', Validators.required],
    nomeBordado: [''],
    observacoes: [''],
  });

  constructor(
    private readonly orderService: OrderService,
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
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

    const details: CustomOrderDetails = {
      tipoPeca: value.tipoPeca,
      tamanho: value.tamanho,
      tecido: value.tecido,
      cor: value.cor,
      nomeBordado: value.nomeBordado,
      observacoes: value.observacoes,
    };

    this.orderService
      .createCustomOrder({
        customerName: value.customerName,
        customerEmail: value.customerEmail,
        customerPhone: value.customerPhone || null,
        notes: 'Encomenda personalizada — valor final a combinar com o ateliê.',
        customDetailsJson: JSON.stringify(details),
        estimatedPrice: 0,
      })
      .subscribe({
        next: (order) => this.router.navigate(['/pedido', order.id]),
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(err?.error?.detail ?? 'Não foi possível enviar sua encomenda. Tente novamente.');
        },
      });
  }
}
