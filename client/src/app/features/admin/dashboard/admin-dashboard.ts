import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Dashboard } from '../../../core/models/dashboard.model';
import { ORDER_STATUS_LABELS } from '../../../core/models/order.model';
import { DashboardService } from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './admin-dashboard.html',
})
export class AdminDashboard implements OnInit {
  readonly dashboard = signal<Dashboard | null>(null);
  readonly loading = signal(true);
  readonly statusLabels = ORDER_STATUS_LABELS;

  constructor(private readonly dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getSummary().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  statusBarWidth(count: number, total: number): string {
    if (total === 0) return '0%';
    return `${Math.round((count / total) * 100)}%`;
  }
}
