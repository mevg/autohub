import { Component, effect, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { VehicleFormValue } from '../vehicle.models';

@Component({
  selector: 'app-vehicle-form-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './vehicle-form-dialog.html',
})
export class VehicleFormDialog {
  title = input('Vehículo');
  /** Valor inicial para editar; vacío para crear. */
  initial = input<VehicleFormValue | null>(null);
  pending = input(false);
  error = input<string | null>(null);
  save = output<VehicleFormValue>();
  cancel = output<void>();

  protected readonly form = new FormBuilder().nonNullable.group({
    vin: ['', [Validators.required, Validators.pattern(/^[A-HJ-NPR-Z0-9]{17}$/)]],
    make: ['', [Validators.required, Validators.maxLength(50)]],
    model: ['', [Validators.required, Validators.maxLength(50)]],
    year: [new Date().getFullYear(), [Validators.required, Validators.min(1900)]],
    price: [0, [Validators.required, Validators.min(1)]],
    mileage: [0, [Validators.required, Validators.min(0)]],
    transmission: ['', [Validators.required, Validators.maxLength(30)]],
    fuelType: ['', [Validators.required, Validators.maxLength(30)]],
    description: [''],
  });

  constructor() {
    effect(() => {
      const value = this.initial();
      if (value) this.form.patchValue({ ...value, description: value.description ?? '' });
    });
  }
}
