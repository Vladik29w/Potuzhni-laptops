import { Data } from "@angular/router";

// Enums
export enum PayEnum {
  Unknown = 0,
  Cash = 1,
  Online = 2
}
export enum DeliveryEnum {
  Unknown = 0,
  PickUp = 1,
  NovaPost = 2,
}
//DTO
export interface CreateOrderDTO {
  cartId: string;
  payMethod: PayEnum;
  deliveryMethod: DeliveryEnum;
  phoneNumber: string;
  email?: string;
  deliveryCityRef?: string;
  deliveryCityName?: string;
  deliveryWarehouseRef?: string;
  deliveryWarehouseName?: string;
}
export interface OrderResponce {
  orderId: string,
  totalPrice: number,
  payMethod: PayEnum,
  deliveryMethod: DeliveryEnum,
  phoneNumber: string,
  email: string,
  paymentId: string,
  paymentUrl: string,
  createdAt: string
}
export interface OrderStatsDTO {
  date: Date;
  quantity: number;
  sum: number;
}
