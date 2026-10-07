import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { BienvenidaPage } from './bienvenida-page';
import { seedSession } from '../../core/auth/auth.service.spec';

describe('BienvenidaPage', () => {
  let fixture: ComponentFixture<BienvenidaPage>;

  beforeEach(async () => {
    localStorage.clear();
    seedSession();
    await TestBed.configureTestingModule({
      imports: [BienvenidaPage],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(BienvenidaPage);
    fixture.detectChanges();
  });

  afterEach(() => localStorage.clear());

  it('greets the current user', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toContain('Hola, Test User');
    expect(el.textContent).toContain('a@b.c');
  });
});
