import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  readonly product = signal<Product | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly quantity = signal(1);
  readonly addedFeedback = signal(false);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly productService: ProductService,
    readonly cart: CartService,
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug')!;
    this.productService.getBySlug(slug).subscribe({
      next: (product) => {
        this.product.set(product);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;

    this.cart.add(product, this.quantity());
    this.addedFeedback.set(true);
    setTimeout(() => this.addedFeedback.set(false), 2500);
  }
}
