import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [RouterLink, CurrencyPipe],
  templateUrl: './shop.html',
})
export class Shop implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly categories = signal<string[]>([]);
  readonly activeCategory = signal<string | null>(null);
  readonly loading = signal(true);

  constructor(
    private readonly productService: ProductService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    readonly cart: CartService,
  ) {}

  ngOnInit(): void {
    this.productService.listCategories().subscribe((categories) => this.categories.set(categories));

    this.route.queryParamMap.subscribe((params) => {
      const category = params.get('categoria');
      this.activeCategory.set(category);
      this.load(category);
    });
  }

  selectCategory(category: string | null): void {
    this.router.navigate([], { queryParams: category ? { categoria: category } : {} });
  }

  addToCart(product: Product): void {
    this.cart.add(product, 1);
  }

  private load(category: string | null): void {
    this.loading.set(true);
    this.productService.list(category ?? undefined).subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
