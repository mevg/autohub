import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/auth/auth.service';
import { BrandingService } from '../../../core/branding/branding.service';
import { LoginForm } from './login-form';

@Component({
  selector: 'app-login-page',
  imports: [LoginForm],
  templateUrl: './login-page.html',
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly branding = inject(BrandingService).branding;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected onSubmit(credentials: { email: string; password: string }): void {
    this.pending.set(true);
    this.error.set(null);
    this.auth.login(credentials).subscribe({
      next: () => {
        this.pending.set(false);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/bienvenida';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (err: unknown) => {
        this.pending.set(false);
        this.error.set(
          err instanceof HttpErrorResponse && err.status === 401
            ? 'Credenciales inválidas.'
            : 'No se pudo ingresar. Intenta de nuevo.',
        );
      },
    });
  }
}
