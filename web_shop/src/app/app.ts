import { Component, Signal, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CartService } from './services/cart.service';
import { AuthService } from './services/auth.service';
import { CommonModule } from '@angular/common';
import { CartDTO } from './DTO/cart-dto';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule],
  styleUrl: './app.css'
})
export class App {
  private authService = inject(AuthService);
  private cartService = inject(CartService);
  cart: Signal<CartDTO | null> = this.cartService.cart;
  showCart: Signal<boolean> = this.cartService.showCart;
  clearCart(): void {
    this.cartService.clearCart().subscribe({})
  }
  removeFromCart(laptopId: string): void {
    this.cartService.removeFromCart(laptopId).subscribe({});
  }
  openCart(): void {
    this.cartService.openCart();
  }
  closeCart(): void {
    this.cartService.closeCart();
  }
  ngOnInit() {
    this.cartService.getCart().subscribe({
      next: (cartData) => {
        console.log('cart:', cartData);
      },
      error: (err) => {
        console.error('cart error:', err);
      }
    });
    //get token
    this.authService.checkUser().subscribe();
  }
}
