import { Component, HostListener, signal } from '@angular/core';

@Component({
  selector: 'app-gallery',
  standalone: true,
  templateUrl: './gallery.html',
})
export class Gallery {
  readonly images = Array.from({ length: 12 }, (_, i) => `https://picsum.photos/seed/atelie-bebe-galeria-${i + 1}/600/700`);
  readonly selectedIndex = signal<number | null>(null);

  open(index: number): void {
    this.selectedIndex.set(index);
  }

  close(): void {
    this.selectedIndex.set(null);
  }

  next(): void {
    const index = this.selectedIndex();
    if (index === null) return;
    this.selectedIndex.set((index + 1) % this.images.length);
  }

  previous(): void {
    const index = this.selectedIndex();
    if (index === null) return;
    this.selectedIndex.set((index - 1 + this.images.length) % this.images.length);
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if (this.selectedIndex() === null) return;

    if (event.key === 'Escape') this.close();
    if (event.key === 'ArrowRight') this.next();
    if (event.key === 'ArrowLeft') this.previous();
  }
}
