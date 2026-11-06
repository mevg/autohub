import { Component, effect, input, OnDestroy, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  PublicCatalogFacets,
  PublicCatalogFilterValue,
  PublicCatalogSort,
  SORT_OPTIONS,
} from '../public-catalog.models';

@Component({
  selector: 'app-catalog-filters',
  imports: [FormsModule],
  templateUrl: './catalog-filters.html',
})
export class CatalogFilters implements OnDestroy {
  filters = input.required<PublicCatalogFilterValue>();
  facets = input.required<PublicCatalogFacets>();
  changed = output<PublicCatalogFilterValue>();

  protected readonly search = signal('');
  protected readonly make = signal('');
  protected readonly model = signal('');
  protected readonly yearFrom = signal<number | null>(null);
  protected readonly yearTo = signal<number | null>(null);
  protected readonly priceFrom = signal<number | null>(null);
  protected readonly priceTo = signal<number | null>(null);
  protected readonly sort = signal<PublicCatalogSort>('newest');
  protected readonly sortOptions = SORT_OPTIONS;

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const filters = this.filters();
      this.search.set(filters.search);
      this.make.set(filters.make);
      this.model.set(filters.model);
      this.yearFrom.set(filters.yearFrom);
      this.yearTo.set(filters.yearTo);
      this.priceFrom.set(filters.priceFrom);
      this.priceTo.set(filters.priceTo);
      this.sort.set(filters.sort);
    });
  }

  ngOnDestroy(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
  }

  protected onSearch(value: string): void {
    this.search.set(value);
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.emit(), 300);
  }

  protected onMake(value: string): void {
    this.make.set(value);
    this.model.set('');
    this.emit();
  }

  protected onModel(value: string): void {
    this.model.set(value);
    this.emit();
  }

  protected onSort(value: string): void {
    this.sort.set(value as PublicCatalogSort);
    this.emit();
  }

  protected clear(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.search.set('');
    this.make.set('');
    this.model.set('');
    this.yearFrom.set(null);
    this.yearTo.set(null);
    this.priceFrom.set(null);
    this.priceTo.set(null);
    this.sort.set('newest');
    this.emit();
  }

  protected emit(): void {
    this.changed.emit({
      search: this.search().trim(),
      make: this.make(),
      model: this.model(),
      yearFrom: this.yearFrom(),
      yearTo: this.yearTo(),
      priceFrom: this.priceFrom(),
      priceTo: this.priceTo(),
      sort: this.sort(),
    });
  }
}
