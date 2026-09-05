import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CustomerSummary } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerAdminService {
  private readonly baseUrl = `${environment.apiUrl}/admin/customers`;

  constructor(private readonly http: HttpClient) {}

  list(): Observable<CustomerSummary[]> {
    return this.http.get<CustomerSummary[]>(this.baseUrl);
  }
}
