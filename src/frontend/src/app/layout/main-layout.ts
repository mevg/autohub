import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { BrandingService } from '../core/branding/branding.service';
import { environment } from '../../environments/environment';
import { Header } from './header';
import { Sidebar } from './sidebar';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, Header, Sidebar],
  templateUrl: './main-layout.html',
})
export class MainLayout {
  private readonly auth = inject(AuthService);
  protected readonly branding = inject(BrandingService).branding;
  protected readonly version = environment.version;
  protected readonly user = this.auth.currentUser;
  protected readonly sidebarOpen = signal(false);

  protected toggleSidebar(): void {
    this.sidebarOpen.update((v) => !v);
  }

  protected closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  protected onLogout(): void {
    this.auth.logout();
  }
}
