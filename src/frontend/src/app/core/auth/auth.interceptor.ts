import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Adjunta el Bearer a /api/* y saca a /login ante 401 (red de seguridad del
 * check proactivo de expiración). No intercepta el propio login para no
 * interferir con el mensaje de "Credenciales inválidas".
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  const token = auth.getToken();
  const withAuth =
    token && req.url.startsWith('/api')
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(withAuth).pipe(
    catchError((err: unknown) => {
      if (
        err instanceof HttpErrorResponse &&
        err.status === 401 &&
        !req.url.endsWith('/api/auth/login')
      ) {
        auth.logout();
      }
      return throwError(() => err);
    }),
  );
};
