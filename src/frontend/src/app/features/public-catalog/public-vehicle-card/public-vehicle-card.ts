import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatMileage, formatPrice } from '../../vehicles/vehicle.models';
import { PublicVehicleCard as PublicVehicleCardModel } from '../public-catalog.models';

@Component({
  selector: 'app-public-vehicle-card',
  imports: [RouterLink],
  templateUrl: './public-vehicle-card.html',
})
export class PublicVehicleCard {
  vehicle = input.required<PublicVehicleCardModel>();
  index = input(0);
  protected readonly price = (value: number) => formatPrice(value);
  protected readonly mileage = (value: number) => formatMileage(value);
}
