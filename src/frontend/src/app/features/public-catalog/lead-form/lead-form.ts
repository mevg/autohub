import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PublicLeadValue } from '../public-catalog.models';

@Component({
  selector: 'app-public-lead-form',
  imports: [ReactiveFormsModule],
  templateUrl: './lead-form.html',
})
export class PublicLeadForm {
  pending = input(false);
  success = input(false);
  error = input<string | null>(null);
  submitted = output<PublicLeadValue>();

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    phone: [
      '',
      [
        Validators.required,
        Validators.pattern(/^[0-9+()\-\s.]+$/),
        (control) => {
          const digits = (control.value ?? '').replace(/\D/g, '');
          return digits.length >= 10 && digits.length <= 15 ? null : { phoneLength: true };
        },
      ],
    ],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    message: ['', [Validators.maxLength(1000)]],
    website: [''],
  });

  protected readonly attempted = signal(false);

  constructor() {
    effect(() => {
      if (this.success()) {
        this.attempted.set(false);
        this.form.reset();
      }
    });
  }

  protected onSubmit(): void {
    this.attempted.set(true);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitted.emit({
      name: value.name.trim(),
      phone: value.phone.trim(),
      email: value.email.trim(),
      message: value.message.trim() || undefined,
      website: value.website,
    });
  }

  protected showError(name: 'name' | 'phone' | 'email' | 'message'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.attempted());
  }
}
