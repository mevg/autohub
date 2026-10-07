import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs';
import { AuthResponse, CurrentUser, LoginRequest, StoredSession } from './auth.models';

const SESSION_KEY = 'autohub.session';
/** Margen para considerar el token expirado antes de su `exp` real. */
const EXPIRY_SKEW_S = 60;

function readSession(): StoredSession | null {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
    return raw ? (JSON.parse(raw) as StoredSession) : null;
  } catch {
    return null;
  }
}

/** Decodifica el payload del JWT sin librerías. Null si no es parseable. */
export function decodeTokenPayload(token: string): { exp?: number } | null {
  try {
    const payload = token.split('.')[1];
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly session = signal<StoredSession | null>(readSession());

  readonly currentUser = computed<CurrentUser | null>(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.hasValidToken());

  login(credentials: LoginRequest) {
    return this.http.post<AuthResponse>('/api/auth/login', credentials).pipe(
      map((res) => ({
        token: res.token,
        user: { id: res.id, email: res.email, fullName: res.fullName, role: res.role },
      })),
      tap((session) => this.setSession(session)),
    );
  }

  /** Refresca el usuario contra /api/auth/me (mantiene el token). */
  fetchMe() {
    return this.http.get<CurrentUser>('/api/auth/me').pipe(
      tap((user) => {
        const token = this.session()?.token;
        if (token) this.setSession({ token, user });
      }),
    );
  }

  logout(navigate = true): void {
    localStorage.removeItem(SESSION_KEY);
    this.session.set(null);
    if (navigate) void this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.hasValidToken() ? (this.session()?.token ?? null) : null;
  }

  hasValidToken(): boolean {
    const token = this.session()?.token;
    if (!token) return false;
    const payload = decodeTokenPayload(token);
    if (!payload?.exp) return false;
    return payload.exp * 1000 - EXPIRY_SKEW_S * 1000 > Date.now();
  }

  private setSession(session: StoredSession): void {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    this.session.set(session);
  }
}
