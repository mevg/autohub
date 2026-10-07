import { APP_INITIALIZER, Provider } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BrandingService } from './branding.service';

/** Aplica el branding antes del primer render; con fallback si no hay backend. */
export const BRANDING_PROVIDERS: Provider[] = [
  {
    provide: APP_INITIALIZER,
    multi: true,
    useFactory: (branding: BrandingService) => () => firstValueFrom(branding.load()),
    deps: [BrandingService],
  },
];
