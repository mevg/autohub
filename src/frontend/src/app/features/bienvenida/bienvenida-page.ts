import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { BrandingService } from '../../core/branding/branding.service';

@Component({
  selector: 'app-bienvenida-page',
  imports: [RouterLink],
  templateUrl: './bienvenida-page.html',
})
export class BienvenidaPage {
  protected readonly user = inject(AuthService).currentUser;
  protected readonly branding = inject(BrandingService).branding;
}
