import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LaptopMainDTO, LaptopDetailsDTO, LaptopAdminDTO, PagedResultDTO } from '../DTO/laptop-dto';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LaptopService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}${environment.endpoints.laptop}`

  getLaptopById(id: string): Observable<LaptopDetailsDTO> {
    return this.http.get<LaptopDetailsDTO>(`${this.url}/${id}`);
  }
  getAllLaptops(page: number, pageSize: number): Observable<PagedResultDTO<LaptopMainDTO>> {
    return this.http.get<PagedResultDTO<LaptopMainDTO>>(
      `${this.url}?page=${page}&pageSize=${pageSize}`
    );
  }
  getAdminLaptops(): Observable<LaptopAdminDTO[]> {
    return this.http.get<LaptopAdminDTO[]>(`${this.url}/admin`);
  }
}
