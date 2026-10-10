import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VEHICLE_STATUSES, STATUS_LABELS } from '../vehicle.models';

export interface FilterValue {
  search: string;
  status: string;
}

@Component({
  selector: 'app-vehicle-filters',
  imports: [FormsModule],
  templateUrl: './vehicle-filters.html',
})
export class VehicleFilters {
  search = input('');
  status = input('');
  changed = output<FilterValue>();
  protected readonly statuses = VEHICLE_STATUSES;
  protected readonly labels = STATUS_LABELS;
}
