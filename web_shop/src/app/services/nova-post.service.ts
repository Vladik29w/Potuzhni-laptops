import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NpSettlementAddress, NpWarehouse } from '../DTO/novapost-dto';

@Injectable({
  providedIn: 'root',
})
export class NovaPostService {
  private readonly url = `${environment.apiUrl}${environment.endpoints.novaPost}`;
  private http = inject(HttpClient);

  getCities(cityName: string): Observable<NpSettlementAddress[]> {
    return this.http.get<NpSettlementAddress[]>(`${this.url}/city`, {
      params: { cityName },
    });
  }

  getWarehouses(cityRef: string, searchString?: string): Observable<NpWarehouse[]> {
    return this.http.get<NpWarehouse[]>(`${this.url}/warehouse/${cityRef}`, {
      params: searchString ? { searchString } : {},
    });
  }
}
