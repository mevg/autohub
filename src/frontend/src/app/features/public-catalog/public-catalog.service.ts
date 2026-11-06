import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { finalize } from 'rxjs';
import {
  DEFAULT_CATALOG_PAGE_SIZE,
  emptyFilterValue,
  PublicCatalogFacets,
  PublicCatalogFilterValue,
  PublicLeadValue,
  PublicPagedResult,
  PublicVehicleCard,
  PublicVehicleDetail,
} from './public-catalog.models';

const API_URL = '/api/public/vehicles';

@Injectable({ providedIn: 'root' })
export class PublicCatalogService {
  private readonly http = inject(HttpClient);
  private listRequestId = 0;

  readonly items = signal<PublicVehicleCard[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(DEFAULT_CATALOG_PAGE_SIZE);
  readonly filters = signal<PublicCatalogFilterValue>(emptyFilterValue());
  readonly facets = signal<PublicCatalogFacets>({
    makes: [],
    models: [],
    minYear: null,
    maxYear: null,
    minPrice: null,
    maxPrice: null,
  });
  readonly loading = signal(false);
  readonly facetsLoading = signal(false);
  readonly error = signal<string | null>(null);

  readonly totalPages = () => Math.max(1, Math.ceil(this.totalCount() / this.pageSize()));

  load(page = this.page()): void {
    const requestId = ++this.listRequestId;
    this.loading.set(true);
    this.error.set(null);
    this.page.set(page);

    this.http
      .get<PublicPagedResult<PublicVehicleCard>>(API_URL, { params: this.toParams() })
      .pipe(finalize(() => requestId === this.listRequestId && this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (requestId !== this.listRequestId) return;
          this.items.set(res.items);
          this.totalCount.set(res.totalCount);
          this.page.set(res.page);
        },
        error: () => {
          if (requestId !== this.listRequestId) return;
          this.error.set('No se pudo cargar el catálogo. Intenta de nuevo.');
        },
      });
  }

  loadFacets(make?: string): void {
    this.facetsLoading.set(true);
    let params = new HttpParams();
    if (make) params = params.set('make', make);

    this.http
      .get<PublicCatalogFacets>(`${API_URL}/filters`, { params })
      .pipe(finalize(() => this.facetsLoading.set(false)))
      .subscribe({
        next: (facets) => this.facets.set(facets),
        error: () => this.facets.set(this.facets()),
      });
  }

  getById(id: string) {
    return this.http.get<PublicVehicleDetail>(`${API_URL}/${id}`);
  }

  submitLead(vehicleId: string, value: PublicLeadValue, idempotencyKey: string) {
    const headers = new HttpHeaders({ 'Idempotency-Key': idempotencyKey });
    return this.http.post<{ id: string }>(
      `${API_URL}/${vehicleId}/leads`,
      { ...value, consentAccepted: false },
      { headers },
    );
  }

  private toParams(): HttpParams {
    const filters = this.filters();
    let params = new HttpParams()
      .set('page', this.page())
      .set('pageSize', this.pageSize())
      .set('sort', filters.sort);

    if (filters.search) params = params.set('search', filters.search);
    if (filters.make) params = params.set('make', filters.make);
    if (filters.model) params = params.set('model', filters.model);
    if (filters.yearFrom != null) params = params.set('yearFrom', filters.yearFrom);
    if (filters.yearTo != null) params = params.set('yearTo', filters.yearTo);
    if (filters.priceFrom != null) params = params.set('priceFrom', filters.priceFrom);
    if (filters.priceTo != null) params = params.set('priceTo', filters.priceTo);
    return params;
  }
}
