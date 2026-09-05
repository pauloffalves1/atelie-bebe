import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Order, ORDER_STATUS_LABELS, OrderStatus } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';
import { Pagination } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-order-list',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, Pagination],
  templateUrl: './admin-order-list.html',
})
export class AdminOrderList implements OnInit {
  readonly orders = signal<Order[]>([]);
  readonly loading = signal(true);
  readonly activeStatus = signal<OrderStatus | null>(null);
  readonly page = signal(1);
  readonly totalPages = signal(0);
  readonly statusLabels = ORDER_STATUS_LABELS;
  readonly statuses: OrderStatus[] = ['Recebido', 'EmProducao', 'Pronto', 'Enviado', 'Entregue', 'Cancelado'];

  constructor(private readonly orderService: OrderService) {}

  ngOnInit(): void {
    this.load();
  }

  filterByStatus(status: OrderStatus | null): void {
    this.activeStatus.set(status);
    this.page.set(1);
    this.load();
  }

  goToPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.orderService.listAllForAdmin(this.activeStatus() ?? undefined, this.page()).subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
