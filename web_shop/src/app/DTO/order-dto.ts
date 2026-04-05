// Enums
export enum PayEnum {
  Unknown = 0,
  Cash = 1,
  Card = 2,
  GooglePay = 3,
  ApplePay = 4
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
export interface OrderStatsDTO {
  date: Date;
  quantity: number;
  sum: number;
}
