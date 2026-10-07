import { Component, input, output, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { LoginRequest } from '../../../core/auth/auth.models';

@Component({
  selector: 'app-login-form',
  imports: [ReactiveFormsModule],
  templateUrl: './login-form.html',
})
export class LoginForm {
  form = input.required<FormGroup>();
  pending = input(false);
  error = input<string | null>(null);
  submitted = output<LoginRequest>();

  protected readonly showPassword = signal(false);

  protected togglePassword(): void {
    this.showPassword.update((v) => !v);
  }
}
