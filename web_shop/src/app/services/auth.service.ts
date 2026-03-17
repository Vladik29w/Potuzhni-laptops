import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { UserDTO, RegisterDTO, LoginDTO } from '../DTO/account-dto';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private url = 'https://localhost:7174/account';

  currentUser = signal<UserDTO | null>(null);
  constructor(private http: HttpClient) { }

  private authenticate<T>(endpoint: 'login' | 'register', data: T): Observable<UserDTO> {
    return this.http.post<UserDTO>(`${this.url}/${endpoint}`, data).pipe(
      tap(user => this.currentUser.set(user))
    );
  }

  login(data: LoginDTO): Observable<UserDTO> {
    return this.authenticate('login', data);
  }
  register(data: RegisterDTO): Observable<UserDTO> {
    return this.authenticate('register', data);
  }

  checkUser(): Observable<UserDTO> {
    return this.http.get<UserDTO>(`${this.url}/me`).pipe(
      tap({
        next: (user) => this.currentUser.set(user),
        error: () => this.currentUser.set(null)
      })
    );
  }
}
