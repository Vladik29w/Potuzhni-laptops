export interface CartItemDTO {
  laptopId: string
  laptopName: string
  price: number
  quantity: number
  totalPrice: number
};
export interface CartDTO {
  cardId: string
  items: CartItemDTO[]
  grandTotal: number
};
