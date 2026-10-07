import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Component } from '@angular/core';
import { authInterceptor } from './auth.interceptor';
import { seedSession } from './auth.service.spec';

@Component({ template: '' })
class DummyLogin {}

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([{ path: 'login', component: DummyLogin }]),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('attaches Bearer to /api requests', () => {
    seedSession();
    http.get('/api/settings/branding').subscribe();
    const req = httpMock.expectOne('/api/settings/branding');
    expect(req.request.headers.get('Authorization')).toMatch(/^Bearer ./);
    req.flush({});
  });

  it('logs out on 401 from non-login endpoints', () => {
    seedSession();
    http.get('/api/settings/branding').subscribe({ error: () => undefined });
    httpMock
      .expectOne('/api/settings/branding')
      .flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(localStorage.getItem('autohub.session')).toBeNull();
  });

  it('does not log out on 401 from login', () => {
    seedSession();
    http.post('/api/auth/login', {}).subscribe({ error: () => undefined });
    httpMock.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(localStorage.getItem('autohub.session')).not.toBeNull();
  });
});
