import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { Pagination } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-admin-product-list',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, Pagination],
  templateUrl: './admin-product-list.html',
})
export class AdminProductList implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly page = signal(1);
  readonly totalPages = signal(0);
  readonly loading = signal(true);

  constructor(private readonly productService: ProductService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService.listAllForAdmin(this.page()).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goToPage(page: number): void {
    this.page.set(page);
    this.load();
  }

  toggleActive(product: Product): void {
    this.productService.setActive(product.id, !product.active).subscribe(() => this.load());
  }
}
