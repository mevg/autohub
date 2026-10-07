import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, of, tap, timeout } from 'rxjs';
import { Branding, DEFAULT_BRANDING } from './branding.model';

const BRANDING_URL = '/api/settings/branding';
const LOAD_TIMEOUT_MS = 5000;

@Injectable({ providedIn: 'root' })
export class BrandingService {
  private readonly http = inject(HttpClient);

  readonly branding = signal<Branding>(DEFAULT_BRANDING);

  /** Carga el branding del backend; si falla usa los defaults. Nunca rechaza. */
  load() {
    return this.http.get<Partial<Branding>>(BRANDING_URL).pipe(
      timeout(LOAD_TIMEOUT_MS),
      catchError(() => of(DEFAULT_BRANDING)),
      map((remote) => ({ ...DEFAULT_BRANDING, ...remote })),
      tap((branding) => {
        this.branding.set(branding);
        this.apply(branding);
      }),
    );
  }

  apply(branding: Branding): void {
    document.title = branding.appName;
    this.setCssVar('--brand-primary-light', branding.primaryLight);
    this.setCssVar('--brand-primary', branding.primary);
    this.setCssVar('--brand-primary-dark', branding.primaryDark);
    this.setFavicon(branding.faviconUrl);
  }

  private setCssVar(name: string, value: string): void {
    document.documentElement.style.setProperty(name, value);
  }

  private setFavicon(href: string): void {
    if (!href) return;
    let link = document.querySelector<HTMLLinkElement>("link[rel='icon']");
    if (!link) {
      link = document.createElement('link');
      link.rel = 'icon';
      document.head.appendChild(link);
    }
    link.href = href;
  }
}
