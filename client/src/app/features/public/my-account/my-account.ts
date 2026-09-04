import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Order, ORDER_STATUS_LABELS } from '../../../core/models/order.model';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-my-account',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './my-account.html',
})
export class MyAccount implements OnInit {
  readonly orders = signal<Order[]>([]);
  readonly loading = signal(true);
  readonly statusLabels = ORDER_STATUS_LABELS;

  constructor(
    readonly auth: AuthService,
    private readonly orderService: OrderService,
  ) {}

  ngOnInit(): void {
    this.orderService.listMine().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
