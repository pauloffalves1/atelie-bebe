import { Component, ElementRef, OnDestroy, OnInit, ViewChild, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('client');

  @ViewChild('cursorGlow', { static: true })
  private readonly cursorGlowRef!: ElementRef<HTMLDivElement>;

  private readonly onMouseMove = (event: MouseEvent): void => {
    const el = this.cursorGlowRef.nativeElement;
    el.style.transform = `translate(${event.clientX}px, ${event.clientY}px)`;
    el.style.opacity = '1';
  };

  ngOnInit(): void {
    window.addEventListener('mousemove', this.onMouseMove);
  }

  ngOnDestroy(): void {
    window.removeEventListener('mousemove', this.onMouseMove);
  }
}
