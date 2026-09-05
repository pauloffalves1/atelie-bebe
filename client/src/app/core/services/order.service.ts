import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateCustomOrderRequest, CreateStoreOrderRequest, Order } from '../models/order.model';
import { PagedResult } from '../models/pagination.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = `${environment.apiUrl}/orders`;
  private readonly adminUrl = `${environment.apiUrl}/admin/orders`;

  constructor(private readonly http: HttpClient) {}

  createStoreOrder(request: CreateStoreOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/store`, request);
  }

  createCustomOrder(request: CreateCustomOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/custom`, request);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  listMine(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.baseUrl}/mine`);
  }

  // ---- admin ----

  listAllForAdmin(status?: string, page = 1, pageSize = 20): Observable<PagedResult<Order>> {
    const params: Record<string, string | number> = { page, pageSize };
    if (status) params['status'] = status;
    return this.http.get<PagedResult<Order>>(this.adminUrl, { params });
  }

  changeStatus(id: string, status: string): Observable<Order> {
    return this.http.patch<Order>(`${this.adminUrl}/${id}/status`, { status });
  }
}
