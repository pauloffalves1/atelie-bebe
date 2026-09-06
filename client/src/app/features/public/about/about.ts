import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
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

  constructor(private readonly siteImageService: SiteImageService) {}

  ngOnInit(): void {
    this.siteImageService.list().subscribe({
      next: (images) => {
        const about = images.find((i) => i.key === 'about');
        if (about) this.imageUrl.set(resolveAssetUrl(about.url));
      },
      error: () => {},
    });
  }
}
