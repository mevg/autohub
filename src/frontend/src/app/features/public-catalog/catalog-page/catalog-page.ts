import { Component, inject } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { BrandingService } from '../../../core/branding/branding.service';
import { Pagination } from '../../vehicles/pagination/pagination';
import { VehicleCardSkeleton } from '../../vehicles/vehicle-card/vehicle-card-skeleton';
import { CatalogFilters } from '../catalog-filters/catalog-filters';
import {
  emptyFilterValue,
  PublicCatalogFilterValue,
  PublicCatalogSort,
} from '../public-catalog.models';
import { PublicCatalogService } from '../public-catalog.service';
import { PublicVehicleCard } from '../public-vehicle-card/public-vehicle-card';

const SORTS: PublicCatalogSort[] = ['newest', 'price_asc', 'price_desc', 'year_desc'];

@Component({
  selector: 'app-catalog-page',
  imports: [CatalogFilters, PublicVehicleCard, VehicleCardSkeleton, Pagination],
  templateUrl: './catalog-page.html',
})
export class CatalogPage {
  protected readonly service = inject(PublicCatalogService);
  protected readonly branding = inject(BrandingService).branding;
  protected readonly emptyFilters = emptyFilterValue();

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);

  constructor() {
    this.title.setTitle(`Autos disponibles | ${this.branding().appName}`);

    this.route.queryParamMap.subscribe((params) => {
      const filters = this.readFilters(params);
      this.service.filters.set(filters);
      this.service.load(this.readPage(params));
      this.service.loadFacets(filters.make);
    });
  }

  protected onFilters(value: PublicCatalogFilterValue): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: this.toQueryParams(value, 1),
    });
  }

  protected onPage(page: number): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: this.toQueryParams(this.service.filters(), page),
    });
  }

  private readFilters(params: ParamMap): PublicCatalogFilterValue {
    const sort = params.get('sort') as PublicCatalogSort | null;
    return {
      search: params.get('search') ?? '',
      make: params.get('make') ?? '',
      model: params.get('model') ?? '',
      yearFrom: this.readNumber(params.get('yearFrom')),
      yearTo: this.readNumber(params.get('yearTo')),
      priceFrom: this.readNumber(params.get('priceFrom')),
      priceTo: this.readNumber(params.get('priceTo')),
      sort: sort && SORTS.includes(sort) ? sort : emptyFilterValue().sort,
    };
  }

  private readPage(params: ParamMap): number {
    const page = this.readNumber(params.get('page'));
    return page && page > 0 ? page : 1;
  }

  private readNumber(value: string | null): number | null {
    if (value == null || value.trim() === '') return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private toQueryParams(
    value: PublicCatalogFilterValue,
    page: number,
  ): Record<string, string | number> {
    const params: Record<string, string | number> = {};
    if (value.search) params['search'] = value.search;
    if (value.make) params['make'] = value.make;
    if (value.model) params['model'] = value.model;
    if (value.yearFrom != null) params['yearFrom'] = value.yearFrom;
    if (value.yearTo != null) params['yearTo'] = value.yearTo;
    if (value.priceFrom != null) params['priceFrom'] = value.priceFrom;
    if (value.priceTo != null) params['priceTo'] = value.priceTo;
    if (value.sort !== 'newest') params['sort'] = value.sort;
    if (page > 1) params['page'] = page;
    return params;
  }
}
