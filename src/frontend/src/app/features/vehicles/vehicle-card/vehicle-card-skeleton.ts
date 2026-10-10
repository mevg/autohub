import { Component } from '@angular/core';

@Component({
  selector: 'app-vehicle-card-skeleton',
  template: `
    <div class="flex flex-col overflow-hidden rounded-2xl bg-white shadow-sm" aria-hidden="true">
      <div class="h-40 animate-pulse bg-gray-200"></div>
      <div class="flex flex-col gap-2 p-4">
        <div class="h-5 w-3/4 animate-pulse rounded bg-gray-200"></div>
        <div class="h-3 w-1/2 animate-pulse rounded bg-gray-200"></div>
        <div class="mt-2 flex items-end justify-between pt-2">
          <div class="h-6 w-1/3 animate-pulse rounded bg-gray-200"></div>
          <div class="h-3 w-1/4 animate-pulse rounded bg-gray-200"></div>
        </div>
      </div>
    </div>
  `,
})
export class VehicleCardSkeleton {}
