import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Order, ORDER_STATUS_LABELS, OrderStatus } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-admin-order-list',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './admin-order-list.html',
})
export class AdminOrderList implements OnInit {
  readonly orders = signal<Order[]>([]);
  readonly loading = signal(true);
  readonly activeStatus = signal<OrderStatus | null>(null);
  readonly statusLabels = ORDER_STATUS_LABELS;
  readonly statuses: OrderStatus[] = ['Recebido', 'EmProducao', 'Pronto', 'Enviado', 'Entregue', 'Cancelado'];

  constructor(private readonly orderService: OrderService) {}

  ngOnInit(): void {
    this.load();
  }

  filterByStatus(status: OrderStatus | null): void {
    this.activeStatus.set(status);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.orderService.listAllForAdmin(this.activeStatus() ?? undefined).subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
