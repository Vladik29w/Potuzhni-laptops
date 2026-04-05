import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CartService } from './cart.service';
import { environment } from '../../environments/environment';
import { CreateOrderDTO } from '../DTO/order-dto';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}${environment.endpoints.order}`;

  private cartService = inject(CartService);

  createOrder(order: CreateOrderDTO) {
    const body = {
      cartId: this.cartService.cartId,
      payMethod: order.payMethod,
      deliveryMethod: order.deliveryMethod,
      phoneNumber: order.phoneNumber,
      email: order.email,
      deliveryCityRef: order.deliveryCityRef,
      deliveryCityName: order.deliveryCityName,
      deliveryWarehouseRef: order.deliveryWarehouseRef,
      deliveryWarehouseName: order.deliveryWarehouseName
    };
    return this.http.post<string>(this.url, body);
  }
}
