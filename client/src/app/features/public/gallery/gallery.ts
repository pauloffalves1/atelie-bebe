import { Component } from '@angular/core';

@Component({
  selector: 'app-gallery',
  standalone: true,
  templateUrl: './gallery.html',
})
export class Gallery {
  readonly images = Array.from({ length: 12 }, (_, i) => `https://picsum.photos/seed/atelie-bebe-galeria-${i + 1}/600/700`);
}
