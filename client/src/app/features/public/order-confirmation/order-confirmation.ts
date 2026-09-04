import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Order, ORDER_STATUS_FLOW, ORDER_STATUS_LABELS } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-order-confirmation',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './order-confirmation.html',
})
export class OrderConfirmation implements OnInit {
  readonly order = signal<Order | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly statusLabels = ORDER_STATUS_LABELS;
  readonly statusFlow = ORDER_STATUS_FLOW;

  private readonly title = inject(Title);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly orderService: OrderService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.orderService.getById(id).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
        this.title.setTitle(`Pedido #${order.id.slice(0, 8)} — Ateliê Bebê`);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  stepIndex(status: string): number {
    return this.statusFlow.indexOf(status as never);
  }
}
