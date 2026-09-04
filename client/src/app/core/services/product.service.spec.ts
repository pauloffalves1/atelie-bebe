import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { ProductService } from './product.service';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('list() requests the public products endpoint without a category filter', () => {
    service.list().subscribe();

    const req = httpMock.expectOne((r) => r.url === `${environment.apiUrl}/products`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.has('category')).toBe(false);
    req.flush([]);
  });

  it('list(category) sends the category as a query param', () => {
    service.list('Toalhas').subscribe();

    const req = httpMock.expectOne((r) => r.url === `${environment.apiUrl}/products`);
    expect(req.request.params.get('category')).toBe('Toalhas');
    req.flush([]);
  });

  it('getBySlug() requests the product by slug', () => {
    service.getBySlug('body-manga-longa-nuvem').subscribe();

    httpMock.expectOne(`${environment.apiUrl}/products/body-manga-longa-nuvem`).flush({});
  });

  it('listAllForAdmin() hits the admin endpoint', () => {
    service.listAllForAdmin().subscribe();

    httpMock.expectOne(`${environment.apiUrl}/admin/products`).flush([]);
  });

  it('updateStock() PATCHes the stock value', () => {
    service.updateStock('p1', 5).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/admin/products/p1/stock`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ stock: 5 });
    req.flush({});
  });
});
