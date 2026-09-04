import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../core/models/product.model';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CurrencyPipe],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  readonly featured = signal<Product[]>([]);
  readonly loading = signal(true);

  constructor(
    private readonly productService: ProductService,
    readonly cart: CartService,
  ) {}

  ngOnInit(): void {
    this.productService.listFeatured().subscribe({
      next: (products) => {
        this.featured.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  addToCart(product: Product): void {
    this.cart.add(product, 1);
  }
}
