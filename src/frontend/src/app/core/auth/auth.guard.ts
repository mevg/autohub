import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';

/** Protege rutas: sin token válido (o sin usuario resoluble) → /login. */
export const authGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.hasValidToken()) {
    auth.logout(false);
    return router.createUrlTree(['/login']);
  }
  if (!auth.currentUser()) {
    try {
      await firstValueFrom(auth.fetchMe());
    } catch {
      return router.createUrlTree(['/login']);
    }
  }
  return true;
};

/** Inverso para /login: con sesión válida → /bienvenida. */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.hasValidToken() ? router.createUrlTree(['/bienvenida']) : true;
};
