import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { CustomerSummary } from '../../../core/models/customer.model';
import { CustomerAdminService } from '../../../core/services/customer-admin.service';

@Component({
  selector: 'app-admin-customer-list',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './admin-customer-list.html',
})
export class AdminCustomerList implements OnInit {
  readonly customers = signal<CustomerSummary[]>([]);
  readonly loading = signal(true);

  constructor(private readonly customerAdminService: CustomerAdminService) {}

  ngOnInit(): void {
    this.customerAdminService.list().subscribe({
      next: (customers) => {
        this.customers.set(customers);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
