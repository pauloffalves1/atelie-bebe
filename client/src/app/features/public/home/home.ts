import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { SeoService } from '../../../core/services/seo.service';
import { SiteImageService } from '../../../core/services/site-image.service';
import { resolveAssetUrl } from '../../../core/utils/asset-url';
import { Product } from '../../../core/models/product.model';
import { AssetUrlPipe } from '../../../shared/pipes/asset-url.pipe';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, AssetUrlPipe],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  readonly featured = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly heroImageUrl = signal('/images/hero-fraldas.jpg');

  constructor(
    private readonly productService: ProductService,
    private readonly siteImageService: SiteImageService,
    private readonly seo: SeoService,
  ) {}

  ngOnInit(): void {
    this.seo.update({
      title: 'Fraldas de ombro e boca bordadas',
      description: 'Fraldas de ombro e boca bordadas à mão para o enxoval do bebê — kits e encomendas personalizadas, feitas com carinho pelo Ateliê Layette Baby.',
      path: '/',
    });

    this.productService.listFeatured().subscribe({
      next: (products) => {
        this.featured.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });

    this.siteImageService.list().subscribe({
      next: (images) => {
        const hero = images.find((i) => i.key === 'home-hero');
        if (hero) this.heroImageUrl.set(resolveAssetUrl(hero.url));
      },
      error: () => {},
    });
  }
}
