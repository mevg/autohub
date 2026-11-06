import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatMileage, formatPrice, VehicleListItem } from '../vehicle.models';
import { StatusBadge } from '../status-badge';

@Component({
  selector: 'app-vehicle-card',
  imports: [RouterLink, StatusBadge],
  templateUrl: './vehicle-card.html',
})
export class VehicleCard {
  vehicle = input.required<VehicleListItem>();
  /** Índice para escalonar la animación de entrada. */
  index = input(0);
  protected readonly price = (v: number) => formatPrice(v);
  protected readonly mileage = (v: number) => formatMileage(v);
}
