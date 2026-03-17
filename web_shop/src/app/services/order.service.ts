import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CartService } from './cart.service';
@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private http = inject(HttpClient);
  private url = 'https://localhost:7174/order';

  private cartService = inject(CartService);

  createOrder(pay: number, delivery: number, phone: string, email: string, address: string) {
    const body = {
      cartId: this.cartService.cartId,
      pay: pay,
      delivery: delivery,
      phone: phone,
      email: email,
      address: address
    };
    return this.http.post<string>(this.url, body);
  }
}
