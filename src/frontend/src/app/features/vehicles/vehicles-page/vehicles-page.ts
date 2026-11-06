import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { VehiclesService } from '../vehicles.service';
import { VehicleCard } from '../vehicle-card/vehicle-card';
import { VehicleCardSkeleton } from '../vehicle-card/vehicle-card-skeleton';
import { FilterValue, VehicleFilters } from '../vehicle-filters/vehicle-filters';
import { Pagination } from '../pagination/pagination';
import { VehicleFormDialog } from '../vehicle-form-dialog/vehicle-form-dialog';
import { VehicleFormValue } from '../vehicle.models';

@Component({
  selector: 'app-vehicles-page',
  imports: [VehicleCard, VehicleCardSkeleton, VehicleFilters, Pagination, VehicleFormDialog],
  templateUrl: './vehicles-page.html',
})
export class VehiclesPage {
  protected readonly service = inject(VehiclesService);
  private readonly router = inject(Router);

  protected readonly showDialog = signal(false);
  protected readonly saving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  constructor() {
    this.service.load(1);
  }

  protected onFilters(value: FilterValue): void {
    this.service.applyFilters({
      search: value.search || undefined,
      status: value.status || undefined,
    });
  }

  protected openDialog(): void {
    this.saveError.set(null);
    this.showDialog.set(true);
  }

  protected onSave(value: VehicleFormValue): void {
    this.saving.set(true);
    this.saveError.set(null);
    this.service.create(value).subscribe({
      next: (res) => {
        this.saving.set(false);
        this.showDialog.set(false);
        void this.router.navigate(['/admin/vehiculos', res.id]);
      },
      error: () => {
        this.saving.set(false);
        this.saveError.set('No se pudo crear el vehículo. Revisa los datos e intenta de nuevo.');
      },
    });
  }
}
