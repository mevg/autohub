import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/public-catalog/public-layout/public-layout').then((m) => m.PublicLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/public-catalog/catalog-page/catalog-page').then((m) => m.CatalogPage),
      },
      {
        path: 'vehiculos/:id',
        loadComponent: () =>
          import('./features/public-catalog/vehicle-detail/vehicle-detail').then(
            (m) => m.PublicVehicleDetailPage,
          ),
      },
    ],
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout').then((m) => m.MainLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'bienvenida' },
      {
        path: 'bienvenida',
        loadComponent: () =>
          import('./features/bienvenida/bienvenida-page').then((m) => m.BienvenidaPage),
      },
      {
        path: 'vehiculos',
        loadComponent: () =>
          import('./features/vehicles/vehicles-page/vehicles-page').then((m) => m.VehiclesPage),
      },
      {
        path: 'vehiculos/:id',
        loadComponent: () =>
          import('./features/vehicles/vehicle-detail-page/vehicle-detail-page').then(
            (m) => m.VehicleDetailPage,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
