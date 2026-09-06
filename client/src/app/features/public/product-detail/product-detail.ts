import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SITE_NAME } from '../../../core/constants/site';
import { Product } from '../../../core/models/product.model';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { AssetUrlPipe } from '../../../shared/pipes/asset-url.pipe';

const MAX_EMBROIDERY_LENGTH = 30;

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CurrencyPipe, FormsModule, RouterLink, AssetUrlPipe],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  readonly alphabet = [..."ABCDEFGHIJKLMNOPQRSTUVWXYZ"];

  readonly product = signal<Product | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly quantity = signal(1);
  readonly embroideryText = signal('');
  readonly embroideryTouched = signal(false);
  readonly addedFeedback = signal(false);

  private readonly title = inject(Title);
  private readonly meta = inject(Meta);

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
        this.title.setTitle(`${product.name} — ${SITE_NAME}`);
        if (product.description) {
          this.meta.updateTag({ name: 'description', content: product.description });
        }
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  appendLetter(letter: string): void {
    if (this.embroideryText().length >= MAX_EMBROIDERY_LENGTH) return;
    this.embroideryText.update((text) => text + letter);
  }

  appendSpace(): void {
    if (this.embroideryText().length >= MAX_EMBROIDERY_LENGTH || this.embroideryText().endsWith(' ')) return;
    this.embroideryText.update((text) => text + ' ');
  }

  removeLastLetter(): void {
    this.embroideryText.update((text) => text.slice(0, -1));
  }

  clearEmbroideryText(): void {
    this.embroideryText.set('');
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;

    if (!this.embroideryText().trim()) {
      this.embroideryTouched.set(true);
      return;
    }

    this.cart.add(product, this.quantity(), this.embroideryText().trim());
    this.addedFeedback.set(true);
    this.embroideryText.set('');
    this.embroideryTouched.set(false);
    setTimeout(() => this.addedFeedback.set(false), 2500);
  }
}
