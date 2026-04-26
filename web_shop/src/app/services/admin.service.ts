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
}
