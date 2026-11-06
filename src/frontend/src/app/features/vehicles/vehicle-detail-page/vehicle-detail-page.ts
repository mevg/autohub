import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { VehiclesService } from '../vehicles.service';
import { StatusBadge } from '../status-badge';
import { VehicleFormDialog } from '../vehicle-form-dialog/vehicle-form-dialog';
import { formatMileage, formatPrice, VehicleDetail, VehicleFormValue } from '../vehicle.models';

@Component({
  selector: 'app-vehicle-detail-page',
  imports: [StatusBadge, VehicleFormDialog],
  templateUrl: './vehicle-detail-page.html',
})
export class VehicleDetailPage {
  private readonly service = inject(VehiclesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly vehicle = signal<VehicleDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly selectedPhoto = signal(0);
  protected readonly showDialog = signal(false);
  protected readonly saving = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly uploading = signal(false);
  protected readonly actionError = signal<string | null>(null);
  protected readonly confirmingArchive = signal(false);

  protected readonly price = (v: number) => formatPrice(v);
  protected readonly mileage = (v: number) => formatMileage(v);

  constructor() {
    this.reload();
  }

  protected reload(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.loading.set(true);
    this.notFound.set(false);
    this.service.getById(id).subscribe({
      next: (v) => {
        this.vehicle.set(v);
        this.selectedPhoto.set(0);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.loading.set(false);
        this.notFound.set(err instanceof HttpErrorResponse && err.status === 404);
      },
    });
  }

  protected onEdit(value: VehicleFormValue): void {
    const current = this.vehicle();
    if (!current) return;
    this.saving.set(true);
    this.saveError.set(null);
    this.service.update(current.id, value).subscribe({
      next: (v) => {
        this.saving.set(false);
        this.showDialog.set(false);
        this.vehicle.set(v);
      },
      error: () => {
        this.saving.set(false);
        this.saveError.set('No se pudo guardar. Revisa los datos e intenta de nuevo.');
      },
    });
  }

  protected onArchive(): void {
    const current = this.vehicle();
    if (!current) return;
    this.service.archive(current.id).subscribe({
      next: (v) => {
        this.vehicle.set(v);
        this.confirmingArchive.set(false);
      },
      error: () => this.actionError.set('No se pudo archivar el vehículo.'),
    });
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const current = this.vehicle();
    input.value = '';
    if (!file || !current) return;
    this.uploading.set(true);
    this.actionError.set(null);
    this.service.uploadImage(current.id, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.reload();
      },
      error: () => {
        this.uploading.set(false);
        this.actionError.set('No se pudo subir la foto (png, jpg o webp, máx. 5 MB).');
      },
    });
  }

  protected onRemoveImage(imageId: string): void {
    const current = this.vehicle();
    if (!current) return;
    this.service.removeImage(current.id, imageId).subscribe({
      next: () => this.reload(),
      error: () => this.actionError.set('No se pudo eliminar la foto.'),
    });
  }

  protected goBack(): void {
    void this.router.navigate(['/admin/vehiculos']);
  }
}
