import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-admin-product-list',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './admin-product-list.html',
})
export class AdminProductList implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);

  constructor(private readonly productService: ProductService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService.listAllForAdmin().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggleActive(product: Product): void {
    this.productService.setActive(product.id, !product.active).subscribe(() => this.load());
  }
}
