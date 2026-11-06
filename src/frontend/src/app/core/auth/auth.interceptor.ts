import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Adjunta el Bearer a /api/* y saca a /login ante 401 (red de seguridad del
 * check proactivo de expiración), conservando la ruta para volver tras el login.
 * No intercepta el propio login para no interferir con "Credenciales inválidas".
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

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
        auth.logout(false);
        void router.navigate(['/login'], {
          queryParams: { returnUrl: router.url, reason: 'expired' },
        });
      }
      return throwError(() => err);
    }),
  );
};
