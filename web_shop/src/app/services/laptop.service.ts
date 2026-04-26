import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LaptopMainDTO, LaptopDetailsDTO, LaptopAdminDTO } from '../DTO/laptop-dto';
import { PageDTO } from '../DTO/page-dto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LaptopService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}${environment.endpoints.laptop}`

  public laptops = signal<LaptopAdminDTO[]>([]);
  constructor() {
    this.getAdminLaptops().subscribe({
      next: (data) => {
        this.laptops.set(data);
      },
    });
  }
  getLaptopById(id: string): Observable<LaptopDetailsDTO> {
    return this.http.get<LaptopDetailsDTO>(`${this.url}/${id}`);
  }
  getAllLaptops(page: number, pageSize: number): Observable<PageDTO<LaptopMainDTO>> {
    return this.http.get<PageDTO<LaptopMainDTO>>(
      `${this.url}?page=${page}&pageSize=${pageSize}`
    );
  }
  saveLaptop(laptop: LaptopAdminDTO) {

    if (laptop.id && laptop.id !== '00000000-0000-0000-0000-000000000000' && laptop.id !== '') {
      return this.http.put(this.url, laptop);
    } else {
      laptop.id = '00000000-0000-0000-0000-000000000000';
      return this.http.post<LaptopAdminDTO>(this.url, laptop);
    }
  }
  deleteLaptop(id: string) {
    return this.http.delete(`${this.url}/${id}`);
  }
  getAdminLaptops(): Observable<LaptopAdminDTO[]> {
    return this.http.get<LaptopAdminDTO[]>(`${this.url}/admin`);
  }
}
