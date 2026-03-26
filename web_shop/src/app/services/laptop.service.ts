import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LaptopMainDTO, LaptopDetailsDTO, LaptopAdminDTO } from '../DTO/laptop-dto';
import { environment } from '../../environments/environment.development'

@Injectable({
  providedIn: 'root'
})
export class LaptopService {
  //back
  private http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/laptop`;

  getLaptopById(id: string): Observable<LaptopDetailsDTO> {
    return this.http.get<LaptopDetailsDTO>(`${this.url}/${id}`);
  }
  getAllLaptops(): Observable<LaptopMainDTO[]> {
    return this.http.get<LaptopMainDTO[]>(this.url);
  }
  getAdminLaptops(): Observable<LaptopAdminDTO[]> {
    return this.http.get<LaptopAdminDTO[]>(`${this.url}/admin`);
  }
}
