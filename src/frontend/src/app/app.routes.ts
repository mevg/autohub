import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout').then((m) => m.MainLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'bienvenida' },
      {
        path: 'bienvenida',
        loadComponent: () =>
          import('./features/bienvenida/bienvenida-page').then((m) => m.BienvenidaPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
