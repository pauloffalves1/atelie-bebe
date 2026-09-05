import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { Pagination } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, Pagination],
  templateUrl: './shop.html',
})
export class Shop implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly categories = signal<string[]>([]);
  readonly activeCategory = signal<string | null>(null);
  readonly page = signal(1);
  readonly totalPages = signal(0);
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
      const page = Number(params.get('pagina')) || 1;
      this.activeCategory.set(category);
      this.page.set(page);
      this.load(category, page);
    });
  }

  selectCategory(category: string | null): void {
    this.router.navigate([], { queryParams: category ? { categoria: category } : {} });
  }

  goToPage(page: number): void {
    const queryParams: Record<string, string | number> = { pagina: page };
    if (this.activeCategory()) queryParams['categoria'] = this.activeCategory()!;
    this.router.navigate([], { queryParams });
  }

  addToCart(product: Product): void {
    this.cart.add(product, 1);
  }

  private load(category: string | null, page: number): void {
    this.loading.set(true);
    this.productService.list(category ?? undefined, page).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
