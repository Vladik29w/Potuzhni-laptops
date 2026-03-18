import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { AuthService } from './services/auth.service';
import { switchMap, catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const authReq = req.clone({
    withCredentials: true
  });
  return next(authReq).pipe(
    catchError((err) => {
      if (err instanceof HttpErrorResponse && err.status === 401 && !req.url.includes('login')) {
        return authService.refresh().pipe(
          switchMap(() => {
            return next(authReq);
          }), 
        )
      }
      return throwError(() => err);
    }) 
  )
}
