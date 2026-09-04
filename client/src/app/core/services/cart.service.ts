import { Injectable, computed, signal } from '@angular/core';
import { CartItem } from '../models/cart.model';
import { Product } from '../models/product.model';

const STORAGE_KEY = 'atelie-bebe.cart';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly itemsSignal = signal<CartItem[]>(this.readStoredCart());

  readonly items = this.itemsSignal.asReadonly();
  readonly totalItems = computed(() => this.itemsSignal().reduce((sum, item) => sum + item.quantity, 0));
  readonly totalPrice = computed(() =>
    this.itemsSignal().reduce((sum, item) => sum + item.product.price * item.quantity, 0),
  );

  add(product: Product, quantity = 1): void {
    const items = [...this.itemsSignal()];
    const existing = items.find((i) => i.product.id === product.id);

    if (existing) {
      existing.quantity = Math.min(existing.quantity + quantity, product.stock);
    } else {
      items.push({ product, quantity: Math.min(quantity, product.stock) });
    }

    this.persist(items);
  }

  updateQuantity(productId: string, quantity: number): void {
    const items = this.itemsSignal()
      .map((item) => (item.product.id === productId ? { ...item, quantity } : item))
      .filter((item) => item.quantity > 0);
    this.persist(items);
  }

  remove(productId: string): void {
    this.persist(this.itemsSignal().filter((item) => item.product.id !== productId));
  }

  clear(): void {
    this.persist([]);
  }

  private persist(items: CartItem[]): void {
    this.itemsSignal.set(items);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }

  private readStoredCart(): CartItem[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as CartItem[]) : [];
    } catch {
      return [];
    }
  }
}
