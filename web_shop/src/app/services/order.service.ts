import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs'
import { environment } from '../../environments/environment';
import { CreateOrderDTO, OrderResponce, OrderStatsDTO, OrderDTO } from '../DTO/order-dto';
import { PageDTO } from '../DTO/page-dto'

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}${environment.endpoints.order}`;

  createOrder(order: CreateOrderDTO) {
    const body = {
      cartId: order.cartId,
      payMethod: order.payMethod,
      deliveryMethod: order.deliveryMethod,
      customerInfo: {
        firstName: order.customerInfo.firstName,
        middleName: order.customerInfo.middleName,
        lastName: order.customerInfo.lastName,
        phoneNumber: order.customerInfo.phoneNumber,
        email: order.customerInfo.email
      },
      deliveryCityRef: order.deliveryCityRef,
      deliveryCityName: order.deliveryCityName,
      deliveryWarehouseRef: order.deliveryWarehouseRef,
      deliveryWarehouseName: order.deliveryWarehouseName
    };
    return this.http.post<OrderResponce>(`${this.url}/create`, body);
  }
  getOrderStats(days: number) {
    return this.http.get<OrderStatsDTO[]>(`${this.url}/stats?days=${days}`);
  }
  getAllOrders(page: number, pageSize: number): Observable<PageDTO<OrderDTO>> {
    return this.http.get<PageDTO<OrderDTO>>(`${this.url}/all?page=${page}&pageSize=${pageSize}`);
  }
  confirmOrder(orderId: string) {
    return this.http.post<void>(`${this.url}/confirm/${orderId}`, null);
  }
}
