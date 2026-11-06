import { Component, computed, input } from '@angular/core';
import { STATUS_LABELS, STATUS_STYLES } from './vehicle.models';

@Component({
  selector: 'app-status-badge',
  template: `<span
    class="inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold {{ styles() }}"
    >{{ label() }}</span
  >`,
})
export class StatusBadge {
  status = input.required<string>();
  protected readonly label = computed(() => STATUS_LABELS[this.status()] ?? this.status());
  protected readonly styles = computed(
    () => STATUS_STYLES[this.status()] ?? 'bg-gray-200 text-gray-700',
  );
}
