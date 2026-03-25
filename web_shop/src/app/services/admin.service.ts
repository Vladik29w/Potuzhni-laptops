import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LaptopAdminDTO } from '../DTO/laptop-dto';
import { OrderStatsDTO } from '../DTO/order-dto';
import { LaptopService } from './laptop.service';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private http = inject(HttpClient);
  private _laptopService = inject(LaptopService);

  private url = 'https://localhost:7174/AdminPanel';

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
    
    if (laptop.id) {
      return this.http.put(`${this.url}/laptop`, laptop);
    } else {
      return this.http.post<LaptopAdminDTO>(`${this.url}/laptop`, laptop);
    }
  }

  deleteLaptop(id: string) {
    return this.http.delete(`${this.url}/laptop/${id}`);
  }
  getOrderStats(days: number) {
    return this.http.get<OrderStatsDTO[]>(`${this.url}/stats?days=${days}`);
  }
}
