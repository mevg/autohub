import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { authGuard, guestGuard } from './auth.guard';
import { fakeJwt, seedSession } from './auth.service.spec';

const FUTURE = Math.floor(Date.now() / 1000) + 3600;
const PAST = Math.floor(Date.now() / 1000) - 3600;

describe('auth guards', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    localStorage.clear();
  });

  const state = (url: string) => ({ url }) as never;

  it('authGuard redirects to /login without session', async () => {
    const result = await TestBed.runInInjectionContext(() =>
      authGuard(null as never, state('/admin/vehiculos')),
    );
    expect(result.toString()).toBe('/login?returnUrl=%2Fadmin%2Fvehiculos&reason=auth');
  });

  it('authGuard redirects to /login with expired token', async () => {
    seedSession(fakeJwt(PAST));
    const result = await TestBed.runInInjectionContext(() =>
      authGuard(null as never, state('/admin/vehiculos')),
    );
    expect(result.toString()).toBe('/login?returnUrl=%2Fadmin%2Fvehiculos&reason=expired');
  });

  it('authGuard allows with valid session', async () => {
    seedSession();
    const result = await TestBed.runInInjectionContext(() =>
      authGuard(null as never, state('/admin/vehiculos')),
    );
    expect(result).toBe(true);
  });

  it('guestGuard redirects to /admin/bienvenida with valid session', async () => {
    seedSession(fakeJwt(FUTURE));
    const result = await TestBed.runInInjectionContext(() =>
      guestGuard(null as never, null as never),
    );
    expect(result.toString()).toBe('/admin/bienvenida');
  });

  it('guestGuard allows /login without session', async () => {
    const result = await TestBed.runInInjectionContext(() =>
      guestGuard(null as never, null as never),
    );
    expect(result).toBe(true);
  });
});
