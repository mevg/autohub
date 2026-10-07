import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';

/** JWT falso con `exp` configurable (segundos desde epoch). */
export function fakeJwt(exp: number): string {
  const b64 = (o: object) =>
    btoa(JSON.stringify(o)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  return `${b64({ alg: 'HS256' })}.${b64({ exp })}`;
}

const FUTURE = Math.floor(Date.now() / 1000) + 3600;
const PAST = Math.floor(Date.now() / 1000) - 3600;

export function seedSession(token = fakeJwt(FUTURE)): void {
  localStorage.setItem(
    'autohub.session',
    JSON.stringify({
      token,
      user: { id: '1', email: 'a@b.c', fullName: 'Test User', role: 'Admin' },
    }),
  );
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('login stores session and signals', () => {
    let done = false;
    service.login({ email: 'a@b.c', password: 'x' }).subscribe(() => (done = true));
    httpMock.expectOne('/api/auth/login').flush({
      id: '1',
      email: 'a@b.c',
      fullName: 'Test User',
      role: 'Admin',
      token: fakeJwt(FUTURE),
    });

    expect(done).toBe(true);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUser()?.fullName).toBe('Test User');
  });

  it('hasValidToken is false when expired', () => {
    seedSession(fakeJwt(PAST));
    expect(TestBed.inject(AuthService).hasValidToken()).toBe(false);
  });

  it('logout clears storage and signals', () => {
    seedSession();
    service.logout(false);
    expect(localStorage.getItem('autohub.session')).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });
});
