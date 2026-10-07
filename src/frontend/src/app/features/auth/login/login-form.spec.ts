import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, Validators } from '@angular/forms';
import { LoginForm } from './login-form';

describe('LoginForm', () => {
  let fixture: ComponentFixture<LoginForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [LoginForm] }).compileComponents();
    fixture = TestBed.createComponent(LoginForm);
    const form = TestBed.inject(FormBuilder).nonNullable.group({
      email: ['a@b.c', [Validators.required, Validators.email]],
      password: ['x', Validators.required],
    });
    fixture.componentRef.setInput('form', form);
    fixture.detectChanges();
  });

  it('emits submitted with form value when valid', () => {
    const emitted: unknown[] = [];
    fixture.componentInstance.submitted.subscribe((v) => emitted.push(v));
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('ngSubmit'));
    expect(emitted).toEqual([{ email: 'a@b.c', password: 'x' }]);
  });

  it('shows the error message', () => {
    fixture.componentRef.setInput('error', 'Credenciales inválidas.');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Credenciales inválidas.',
    );
  });

  it('disables submit when form is invalid', () => {
    fixture.componentInstance.form().setValue({ email: 'no-es-email', password: '' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(true);
  });

  it('toggles password visibility', () => {
    const input = () =>
      fixture.nativeElement.querySelector('input[formControlName="password"]') as HTMLInputElement;
    const toggle = () =>
      fixture.nativeElement.querySelector('button[aria-label]') as HTMLButtonElement;

    expect(input().type).toBe('password');
    toggle().click();
    fixture.detectChanges();
    expect(input().type).toBe('text');
    expect(toggle().getAttribute('aria-label')).toBe('Ocultar contraseña');
    toggle().click();
    fixture.detectChanges();
    expect(input().type).toBe('password');
  });
});
