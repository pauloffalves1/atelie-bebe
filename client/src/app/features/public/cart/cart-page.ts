import { CurrencyPipe } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart-page.html',
})
export class CartPage {
  constructor(readonly cart: CartService) {}

  increment(productId: string, current: number, max: number): void {
    if (current < max) this.cart.updateQuantity(productId, current + 1);
  }

  decrement(productId: string, current: number): void {
    this.cart.updateQuantity(productId, current - 1);
  }
}
