import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/pagination.model';
import { CreateProductRequest, Product, UpdateProductRequest } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly baseUrl = `${environment.apiUrl}/products`;
  private readonly adminUrl = `${environment.apiUrl}/admin/products`;

  constructor(private readonly http: HttpClient) {}

  list(category?: string, page = 1, pageSize = 12): Observable<PagedResult<Product>> {
    const params: Record<string, string | number> = { page, pageSize };
    if (category) params['category'] = category;
    return this.http.get<PagedResult<Product>>(this.baseUrl, { params });
  }

  listFeatured(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/featured`);
  }

  listCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/categories`);
  }

  getBySlug(slug: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${slug}`);
  }

  // ---- admin ----

  listAllForAdmin(page = 1, pageSize = 20): Observable<PagedResult<Product>> {
    return this.http.get<PagedResult<Product>>(this.adminUrl, { params: { page, pageSize } });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.adminUrl}/${id}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.adminUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.adminUrl}/${id}`, request);
  }

  updateStock(id: string, stock: number): Observable<Product> {
    return this.http.patch<Product>(`${this.adminUrl}/${id}/stock`, { stock });
  }

  setActive(id: string, active: boolean): Observable<Product> {
    return this.http.patch<Product>(`${this.adminUrl}/${id}/active?active=${active}`, {});
  }
}
