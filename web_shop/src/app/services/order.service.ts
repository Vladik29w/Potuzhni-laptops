import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CartService } from './cart.service';
import { environment } from '../../environments/environment.development'
@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/order`;

  private cartService = inject(CartService);

  createOrder(pay: number, delivery: number, phone: string, email: string, address: string) {
    const body = {
      cartId: this.cartService.cartId,
      payMethod: pay,            
      deliveryMethod: delivery,  
      phoneNumber: phone,        
      email: email,              
      shippingAddress: address   
    };
    return this.http.post<string>(this.url, body);
  }
}
