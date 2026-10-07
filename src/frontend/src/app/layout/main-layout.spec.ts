import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { MainLayout } from './main-layout';
import { seedSession } from '../core/auth/auth.service.spec';

@Component({ template: '' })
class Dummy {}

describe('MainLayout', () => {
  let fixture: ComponentFixture<MainLayout>;

  beforeEach(async () => {
    localStorage.clear();
    seedSession();
    await TestBed.configureTestingModule({
      imports: [MainLayout],
      providers: [provideHttpClient(), provideRouter([{ path: '**', component: Dummy }])],
    }).compileComponents();
    fixture = TestBed.createComponent(MainLayout);
    fixture.detectChanges();
  });

  afterEach(() => localStorage.clear());

  it('renders header with user and sidebar link', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('Test User');
    expect(el.textContent).toContain('Admin');
    expect(el.querySelector('a[href="/bienvenida"]')?.textContent).toContain('Bienvenida');
  });

  it('toggles sidebar from header button', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('aside')?.classList).not.toContain('translate-x-0');
    (el.querySelector('header button') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(el.querySelector('aside')?.classList).toContain('translate-x-0');
  });

  it('renders footer with version and credits', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('footer')?.textContent).toContain('v0.0.0');
    expect(el.querySelector('footer')?.textContent).toContain('Develop by MEVG');
  });
});
