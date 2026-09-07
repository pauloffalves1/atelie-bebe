import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { ProductReview, ReviewEligibility } from '../../../core/models/review.model';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { ReviewService } from '../../../core/services/review.service';
import { SeoService } from '../../../core/services/seo.service';
import { resolveAssetUrl } from '../../../core/utils/asset-url';
import { AssetUrlPipe } from '../../../shared/pipes/asset-url.pipe';

const MAX_EMBROIDERY_LENGTH = 30;

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, RouterLink, AssetUrlPipe],
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
  readonly activeImageIndex = signal(0);

  readonly galleryUrls = computed(() => {
    const p = this.product();
    if (!p) return [];
    return [...(p.imageUrl ? [p.imageUrl] : []), ...p.imageUrls];
  });

  readonly reviews = signal<ProductReview[]>([]);
  readonly eligibility = signal<ReviewEligibility | null>(null);
  readonly reviewRating = signal(5);
  readonly reviewComment = signal('');
  readonly submittingReview = signal(false);
  readonly reviewError = signal<string | null>(null);

  readonly averageRating = computed(() => {
    const list = this.reviews();
    return list.length ? list.reduce((sum, r) => sum + r.rating, 0) / list.length : 0;
  });

  private readonly seo = inject(SeoService);
  private readonly auth = inject(AuthService);
  private readonly reviewService = inject(ReviewService);

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
        this.activeImageIndex.set(0);
        this.seo.update({
          title: product.name,
          description: product.description || `${product.name} — peça bordada do Ateliê Layette Baby, feita sob medida com carinho.`,
          path: `/produto/${product.slug}`,
          image: product.imageUrl ? resolveAssetUrl(product.imageUrl) : undefined,
          type: 'product',
        });

        this.reviewService.listByProduct(product.id).subscribe((reviews) => this.reviews.set(reviews));

        if (this.auth.currentUser()) {
          this.reviewService.getEligibility(product.id).subscribe({
            next: (eligibility) => this.eligibility.set(eligibility),
            error: () => {},
          });
        }
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  selectImage(index: number): void {
    this.activeImageIndex.set(index);
  }

  setReviewRating(rating: number): void {
    this.reviewRating.set(rating);
  }

  submitReview(): void {
    const product = this.product();
    if (!product) return;

    this.submittingReview.set(true);
    this.reviewError.set(null);

    this.reviewService.create(product.id, { rating: this.reviewRating(), comment: this.reviewComment().trim() || null }).subscribe({
      next: (review) => {
        this.reviews.update((list) => [review, ...list]);
        this.eligibility.update((current) => (current ? { ...current, alreadyReviewed: true } : current));
        this.reviewComment.set('');
        this.reviewRating.set(5);
        this.submittingReview.set(false);
      },
      error: (err) => {
        this.submittingReview.set(false);
        this.reviewError.set(err?.error?.detail ?? 'Não foi possível enviar sua avaliação.');
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
