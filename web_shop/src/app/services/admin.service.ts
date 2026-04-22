import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LaptopAdminDTO } from '../DTO/laptop-dto';
import { OrderStatsDTO } from '../DTO/order-dto';
import { LaptopService } from './laptop.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private http = inject(HttpClient);
  private _laptopService = inject(LaptopService);

  private laptopUrl = `${environment.apiUrl}${environment.endpoints.laptop}`;
  private orderUrl = `${environment.apiUrl}${environment.endpoints.order}`;

  public laptops = signal<LaptopAdminDTO[]>([]);

  constructor() {
    this.loadLaptops();
  }

  loadLaptops() {
    this._laptopService.getAdminLaptops().subscribe({
      next: (data) => {
        this.laptops.set(data);
      },
    });
  }

  saveLaptop(laptop: LaptopAdminDTO) {

    if (laptop.id && laptop.id !== '00000000-0000-0000-0000-000000000000' && laptop.id !== '') {
      return this.http.put(this.laptopUrl, laptop);
    } else {
      laptop.id = '00000000-0000-0000-0000-000000000000';
      return this.http.post<LaptopAdminDTO>(this.laptopUrl, laptop);
    }
  }

  deleteLaptop(id: string) {
    return this.http.delete(`${this.laptopUrl}/${id}`);
  }
  getOrderStats(days: number) {
    return this.http.get<OrderStatsDTO[]>(`${this.orderUrl}/stats?days=${days}`);
  }
}
