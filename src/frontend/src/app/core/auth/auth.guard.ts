import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';

/** Protege rutas: sin token válido (o sin usuario resoluble) → /login con returnUrl. */
export const authGuard: CanActivateFn = async (
  _route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.hasValidToken()) {
    const reason = auth.isTokenExpired() ? 'expired' : 'auth';
    auth.logout(false);
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url, reason } });
  }
  if (!auth.currentUser()) {
    try {
      await firstValueFrom(auth.fetchMe());
    } catch {
      return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
    }
  }
  return true;
};

/** Inverso para /login: con sesión válida → /bienvenida (o returnUrl). */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.hasValidToken() ? router.createUrlTree(['/bienvenida']) : true;
};
