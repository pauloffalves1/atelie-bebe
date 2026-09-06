import { CurrencyPipe } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { AssetUrlPipe } from '../../../shared/pipes/asset-url.pipe';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, AssetUrlPipe],
  templateUrl: './cart-page.html',
})
export class CartPage {
  constructor(readonly cart: CartService) {}

  increment(productId: string, current: number, embroideryText?: string | null): void {
    this.cart.updateQuantity(productId, current + 1, embroideryText);
  }

  decrement(productId: string, current: number, embroideryText?: string | null): void {
    this.cart.updateQuantity(productId, current - 1, embroideryText);
  }
}
