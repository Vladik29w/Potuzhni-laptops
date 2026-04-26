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
export enum PaymentStatus {
  Pending = 0,
  Success = 1,
  Failure = 2,
  Expired = 3,
  Reversed = 4
}
//DTO
export interface OrderDTO {
  id: string;
  orderItems: OrderItemDTO[];
  totalPrice: number;
  payMethod: PayEnum;
  deliveryMethod: DeliveryEnum;
  isConfirmed: boolean;
  customerInfo: CustomerInfoDTO;
  deliveryCityRef?: string;
  deliveryCityName?: string;
  deliveryWarehouseRef?: string;
  deliveryWarehouseName?: string;
  paymentId?: string;
  paymentUrl?: string;
  paymentStatus: PaymentStatus;
  createdAt: string;
}
export interface OrderItemDTO {
  id: number;
  laptopId: string;
  laptopName: string;
  price: number;
  quantity: number;
  orderId: string;
}
export interface CreateOrderDTO {
  cartId: string;
  payMethod: PayEnum;
  deliveryMethod: DeliveryEnum;
  customerInfo: CustomerInfoDTO;
  deliveryCityRef?: string;
  deliveryCityName?: string;
  deliveryWarehouseRef?: string;
  deliveryWarehouseName?: string;
}
export interface OrderResponce {
  orderId: string;
  totalPrice: number;
  payMethod: PayEnum;
  deliveryMethod: DeliveryEnum;
  customerInfo: CustomerInfoDTO;
  paymentId: string;
  paymentUrl: string;
  createdAt: string
}
export interface OrderStatsDTO {
  date: Date;
  quantity: number;
  sum: number;
}
//complex type
export interface CustomerInfoDTO {
  firstName: string;
  middleName: string;
  lastName: string;
  phoneNumber: string;
  email?: string;
}
