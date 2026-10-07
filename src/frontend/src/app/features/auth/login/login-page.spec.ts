import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { vi } from 'vitest';
import { LoginPage } from './login-page';
import { fakeJwt } from '../../../core/auth/auth.service.spec';

const FUTURE = Math.floor(Date.now() / 1000) + 3600;

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(LoginPage);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  function fillAndSubmit(): void {
    const el = fixture.nativeElement as HTMLElement;
    (el.querySelector('input[type="email"]') as HTMLInputElement).value = 'a@b.c';
    (el.querySelector('input[type="email"]') as HTMLInputElement).dispatchEvent(new Event('input'));
    (el.querySelector('input[type="password"]') as HTMLInputElement).value = 'secret';
    (el.querySelector('input[type="password"]') as HTMLInputElement).dispatchEvent(
      new Event('input'),
    );
    fixture.detectChanges();
    (el.querySelector('button[type="submit"]') as HTMLButtonElement).click();
  }

  it('navigates to /bienvenida on success', () => {
    const nav = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fillAndSubmit();
    httpMock.expectOne('/api/auth/login').flush({
      id: '1',
      email: 'a@b.c',
      fullName: 'Test User',
      role: 'Admin',
      token: fakeJwt(FUTURE),
    });
    expect(nav).toHaveBeenCalledWith('/bienvenida');
  });

  it('shows error on 401', () => {
    fillAndSubmit();
    httpMock.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Credenciales inválidas.',
    );
  });
});
