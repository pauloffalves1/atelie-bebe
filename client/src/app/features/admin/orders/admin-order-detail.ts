import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CustomOrderDetails, Order, ORDER_STATUS_LABELS, OrderItemOptions, OrderStatus, ShippingAddress } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';
import { CpfMaskPipe } from '../../../shared/pipes/cpf-mask.pipe';

const ALLOWED_TRANSITIONS: Record<OrderStatus, OrderStatus[]> = {
  Recebido: ['EmProducao', 'Cancelado'],
  EmProducao: ['Pronto', 'Cancelado'],
  Pronto: ['Enviado', 'Cancelado'],
  Enviado: ['Entregue'],
  Entregue: [],
  Cancelado: [],
};

@Component({
  selector: 'app-admin-order-detail',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, CpfMaskPipe],
  templateUrl: './admin-order-detail.html',
})
export class AdminOrderDetail implements OnInit {
  readonly order = signal<Order | null>(null);
  readonly loading = signal(true);
  readonly updating = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly statusLabels = ORDER_STATUS_LABELS;

  private orderId!: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly orderService: OrderService,
  ) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  get availableTransitions(): OrderStatus[] {
    const order = this.order();
    return order ? ALLOWED_TRANSITIONS[order.status] : [];
  }

  changeStatus(status: OrderStatus): void {
    this.updating.set(true);
    this.errorMessage.set(null);

    this.orderService.changeStatus(this.orderId, status).subscribe({
      next: (order) => {
        this.order.set(order);
        this.updating.set(false);
      },
      error: (err) => {
        this.updating.set(false);
        this.errorMessage.set(err?.error?.detail ?? 'Não foi possível atualizar o status.');
      },
    });
  }

  parsedCustomDetails(): CustomOrderDetails | null {
    const json = this.order()?.customDetailsJson;
    if (!json) return null;
    try {
      return JSON.parse(json) as CustomOrderDetails;
    } catch {
      return null;
    }
  }

  parsedItemOptions(optionsJson: string | null): OrderItemOptions | null {
    if (!optionsJson) return null;
    try {
      return JSON.parse(optionsJson) as OrderItemOptions;
    } catch {
      return null;
    }
  }

  parsedShippingAddress(): ShippingAddress | null {
    const json = this.order()?.shippingAddressJson;
    if (!json) return null;
    try {
      return JSON.parse(json) as ShippingAddress;
    } catch {
      return null;
    }
  }

  private load(): void {
    this.loading.set(true);
    this.orderService.getById(this.orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
