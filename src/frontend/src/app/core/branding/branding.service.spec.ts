import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { BrandingService } from './branding.service';
import { DEFAULT_BRANDING } from './branding.model';

describe('BrandingService', () => {
  let service: BrandingService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(BrandingService);
    httpMock = TestBed.inject(HttpTestingController);
    document.title = '';
    document.documentElement.removeAttribute('style');
  });

  afterEach(() => httpMock.verify());

  it('applies remote branding on successful load', async () => {
    const promise = firstValueFrom(service.load());
    httpMock
      .expectOne('/api/settings/branding')
      .flush({ appName: 'Mi Tienda', primary: '#FF8840' });

    const branding = await promise;

    expect(branding.appName).toBe('Mi Tienda');
    expect(service.branding().appName).toBe('Mi Tienda');
    expect(document.title).toBe('Mi Tienda');
    expect(document.documentElement.style.getPropertyValue('--brand-primary')).toBe('#FF8840');
    // Los campos no enviados conservan los defaults.
    expect(branding.primaryLight).toBe(DEFAULT_BRANDING.primaryLight);
  });

  it('falls back to defaults when backend is unreachable', async () => {
    const promise = firstValueFrom(service.load());
    httpMock.expectOne('/api/settings/branding').error(new ProgressEvent('error'));

    const branding = await promise;

    expect(branding).toEqual(DEFAULT_BRANDING);
    expect(document.title).toBe(DEFAULT_BRANDING.appName);
    expect(document.documentElement.style.getPropertyValue('--brand-primary')).toBe(
      DEFAULT_BRANDING.primary,
    );
  });

  it('updates favicon from branding', async () => {
    const promise = firstValueFrom(service.load());
    httpMock.expectOne('/api/settings/branding').flush({ faviconUrl: '/brand/icon.png' });
    await promise;

    expect(document.querySelector("link[rel='icon']")?.getAttribute('href')).toBe(
      '/brand/icon.png',
    );
  });
});
