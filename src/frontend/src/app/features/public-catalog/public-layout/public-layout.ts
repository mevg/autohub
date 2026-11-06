import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { BrandingService } from '../../../core/branding/branding.service';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './public-layout.html',
})
export class PublicLayout {
  protected readonly branding = inject(BrandingService).branding;
  protected readonly year = new Date().getFullYear();
}
