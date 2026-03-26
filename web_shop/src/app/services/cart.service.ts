import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CartItemDTO, CartDTO } from '../DTO/cart-dto'
import { tap } from 'rxjs';
import { environment } from '../../environments/environment.development'

@Injectable({
  providedIn: 'root',
})
export class CartService {
  //back
  private http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/cart`;
  //cart
  private cartSignal = signal<CartDTO | null>(null);
  public  cart = this.cartSignal;
  public showCart = signal<boolean>(false);
  public cartId: string;

  constructor() {
    this.cartId = this.getCartId();
  }
  getCartId() {
    let id = localStorage.getItem("cartId");
    if (id == null) {
      id = crypto.randomUUID();
      localStorage.setItem("cartId", id);
    }
    console.log("cart id:" + id);//del
    return id;
  }
  getCart() {
    return this.http.get<CartDTO>(`${this.url}/${this.cartId}`).pipe(
      tap((gCart) => {
        this.cart.set(gCart)
    })
    )
  }
  addToCart(laptopId: string) {
    console.log("added laptop:" + laptopId);//del
    return this.http.post<CartDTO>(`${this.url}/${this.cartId}/${laptopId}`, {}).pipe(
      tap((updCart) => {
        this.cart.set(updCart)
      })
    );
  }
  clearCart() {
    return this.http.delete<CartDTO>(`${this.url}/${this.cartId}`, {}).pipe(
      tap((updCart) => {
        this.cart.set(updCart)
      })
    );
  }
  removeFromCart(laptopId: string) {
    console.log("deleted: " + laptopId)
    return this.http.delete<CartDTO>(`${this.url}/${this.cartId}/${laptopId}`).pipe(
      tap((updCart) => {
        this.cart.set(updCart)
      })
    );
  }
  //UI
  openCart() {
    this.showCart.set(true);
  }
  closeCart() { 
    this.showCart.set(false);
  }
}
