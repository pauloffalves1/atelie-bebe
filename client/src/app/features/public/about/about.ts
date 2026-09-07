import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SeoService } from '../../../core/services/seo.service';
import { SiteImageService } from '../../../core/services/site-image.service';
import { resolveAssetUrl } from '../../../core/utils/asset-url';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './about.html',
})
export class About implements OnInit {
  readonly imageUrl = signal('/images/sobre-fraldas.png');

  constructor(
    private readonly siteImageService: SiteImageService,
    private readonly seo: SeoService,
  ) {}

  ngOnInit(): void {
    this.seo.update({
      title: 'Sobre o ateliê',
      description: 'Conheça a história do Ateliê Layette Baby: fraldas de ombro e boca costuradas à mão, com tecidos selecionados e bordados feitos com carinho.',
      path: '/sobre',
    });

    this.siteImageService.list().subscribe({
      next: (images) => {
        const about = images.find((i) => i.key === 'about');
        if (about) this.imageUrl.set(resolveAssetUrl(about.url));
      },
      error: () => {},
    });
  }
}
