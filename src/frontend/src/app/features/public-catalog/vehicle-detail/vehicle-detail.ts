import { Component, DOCUMENT, inject, OnDestroy, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BrandingService } from '../../../core/branding/branding.service';
import { formatMileage, formatPrice } from '../../vehicles/vehicle.models';
import { PublicLeadForm } from '../lead-form/lead-form';
import { PublicLeadValue, PublicVehicleDetail } from '../public-catalog.models';
import { PublicCatalogService } from '../public-catalog.service';

const JSON_LD_ID = 'vehicle-json-ld';

@Component({
  selector: 'app-public-vehicle-detail',
  imports: [RouterLink, PublicLeadForm],
  templateUrl: './vehicle-detail.html',
})
export class PublicVehicleDetailPage implements OnDestroy {
  protected readonly vehicle = signal<PublicVehicleDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly selectedPhoto = signal(0);
  protected readonly leadPending = signal(false);
  protected readonly leadSuccess = signal(false);
  protected readonly leadError = signal<string | null>(null);

  protected readonly branding = inject(BrandingService).branding;

  private readonly service = inject(PublicCatalogService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly document = inject(DOCUMENT);
  private idempotencyKey: string | null = null;

  constructor() {
    this.route.paramMap.subscribe((params) => this.load(params.get('id')));
  }

  ngOnDestroy(): void {
    this.document.getElementById(JSON_LD_ID)?.remove();
  }

  protected price(value: number): string {
    return formatPrice(value);
  }

  protected mileage(value: number): string {
    return formatMileage(value);
  }

  protected goBack(): void {
    void this.router.navigate(['/']);
  }

  protected onSubmitLead(value: PublicLeadValue): void {
    const vehicle = this.vehicle();
    if (!vehicle) return;

    this.idempotencyKey ??= crypto.randomUUID();
    this.leadPending.set(true);
    this.leadSuccess.set(false);
    this.leadError.set(null);

    this.service.submitLead(vehicle.id, value, this.idempotencyKey).subscribe({
      next: () => {
        this.leadPending.set(false);
        this.leadSuccess.set(true);
        this.idempotencyKey = null;
      },
      error: (err: unknown) => {
        this.leadPending.set(false);
        this.leadError.set(this.leadErrorMessage(err));
        if (err instanceof HttpErrorResponse && (err.status === 400 || err.status === 409)) {
          this.idempotencyKey = null;
        }
      },
    });
  }

  private load(id: string | null): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.leadSuccess.set(false);
    this.leadError.set(null);
    this.idempotencyKey = null;

    if (!id) {
      this.loading.set(false);
      this.notFound.set(true);
      return;
    }

    this.service.getById(id).subscribe({
      next: (vehicle) => {
        this.vehicle.set(vehicle);
        this.selectedPhoto.set(0);
        this.loading.set(false);
        this.updateMetadata(vehicle);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.notFound.set(err instanceof HttpErrorResponse && err.status === 404);
      },
    });
  }

  private leadErrorMessage(err: unknown): string {
    if (!(err instanceof HttpErrorResponse)) {
      return 'No se pudo enviar tu solicitud. Intenta de nuevo.';
    }

    if (err.status === 429) {
      return 'Recibimos varias solicitudes recientes. Espera unos minutos antes de intentar de nuevo.';
    }
    if (err.status === 409) {
      return 'Esta solicitud ya fue registrada. Si necesitas cambiar tus datos, intenta de nuevo.';
    }
    if (err.status === 404) {
      return 'El vehículo ya no está disponible.';
    }
    if (err.status === 400) {
      return 'Revisa tus datos e intenta de nuevo.';
    }
    return 'No se pudo enviar tu solicitud. Intenta de nuevo en unos momentos.';
  }

  private updateMetadata(vehicle: PublicVehicleDetail): void {
    const appName = this.branding().appName;
    const name = `${vehicle.make} ${vehicle.model} ${vehicle.year}`;
    const description =
      vehicle.description?.trim() ||
      `${name} disponible en ${appName}. Referencia ${vehicle.publicReference}.`;

    this.title.setTitle(`${name} | ${appName}`);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: `${name} | ${appName}` });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'product' });
    if (vehicle.images[0]?.imageUrl) {
      this.meta.updateTag({ property: 'og:image', content: vehicle.images[0].imageUrl });
    }

    const jsonLd = {
      '@context': 'https://schema.org',
      '@type': 'Car',
      name,
      sku: vehicle.publicReference,
      brand: { '@type': 'Brand', name: vehicle.make },
      model: vehicle.model,
      vehicleModelDate: String(vehicle.year),
      mileageFromOdometer: {
        '@type': 'QuantitativeValue',
        value: vehicle.mileage,
        unitCode: 'KMT',
      },
      image: vehicle.images.map((image) => image.imageUrl),
      description,
      offers: {
        '@type': 'Offer',
        price: vehicle.price,
        priceCurrency: 'MXN',
        availability: 'https://schema.org/InStock',
        url: this.document.location.href,
      },
    };

    let script = this.document.getElementById(JSON_LD_ID) as HTMLScriptElement | null;
    if (!script) {
      script = this.document.createElement('script');
      script.id = JSON_LD_ID;
      script.type = 'application/ld+json';
      this.document.head.appendChild(script);
    }
    script.textContent = JSON.stringify(jsonLd);
  }
}
