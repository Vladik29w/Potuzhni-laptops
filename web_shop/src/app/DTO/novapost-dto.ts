// Base DTO
export interface NpReq<T> {
  apiKey: string;
  modelName: string;
  calledMethod: string;
  methodProperties: T;
}
export interface NpResponse<T> {
  success: boolean;
  data: T[];
  errors: string[];
}
// Settlements DTO
export interface NpSettlementsReq {
  CityName: string;
  Limit?: string;
  Page?: string;
}
export interface NpSettlementsData {
  TotalCount: number;
  Addresses: NpSettlementAddress[];
}
export interface NpSettlementAddress {
  Present: string;
  DeliveryCity: string;
  Ref: string;
  SettlementRef?: string;
  MainDescription: string;
}
// Warehouse DTO
export interface NpGetWarehouseReq {
  SettlementRef: string;
  CityRef?: string;
  FindByString?: string;
  Page?: string;
  Limit?: string;
}
export interface NpWarehouse {
  Description?: string;
  Ref: string;
  SettlementRef: string;
  TypeOfWarehouseRef?: string;
}
