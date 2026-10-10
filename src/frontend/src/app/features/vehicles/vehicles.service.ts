import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { finalize } from 'rxjs';
import {
  PagedResult,
  VehicleDetail,
  VehicleFilters,
  VehicleFormValue,
  VehicleImage,
  VehicleListItem,
} from './vehicle.models';

const API_URL = '/api/vehicles';
const DEFAULT_PAGE_SIZE = 12;

@Injectable({ providedIn: 'root' })
export class VehiclesService {
  private readonly http = inject(HttpClient);

  readonly items = signal<VehicleListItem[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(DEFAULT_PAGE_SIZE);
  readonly filters = signal<Omit<VehicleFilters, 'page' | 'pageSize'>>({});
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly totalPages = () => Math.max(1, Math.ceil(this.totalCount() / this.pageSize()));

  load(page = this.page()): void {
    this.loading.set(true);
    this.error.set(null);
    this.page.set(page);
    this.http
      .get<PagedResult<VehicleListItem>>(API_URL, { params: this.toParams() })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.items.set(res.items);
          this.totalCount.set(res.totalCount);
        },
        error: () => this.error.set('No se pudieron cargar los vehículos. Intenta de nuevo.'),
      });
  }

  applyFilters(filters: Omit<VehicleFilters, 'page' | 'pageSize'>): void {
    this.filters.set(filters);
    this.load(1);
  }

  getById(id: string) {
    return this.http.get<VehicleDetail>(`${API_URL}/${id}`);
  }

  create(value: VehicleFormValue) {
    return this.http.post<{ id: string }>(API_URL, value);
  }

  update(id: string, value: VehicleFormValue) {
    return this.http.put<VehicleDetail>(`${API_URL}/${id}`, value);
  }

  archive(id: string) {
    return this.http.post<VehicleDetail>(`${API_URL}/${id}/archive`, {});
  }

  uploadImage(id: string, file: File) {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<VehicleImage>(`${API_URL}/${id}/images`, form);
  }

  removeImage(vehicleId: string, imageId: string) {
    return this.http.delete(`${API_URL}/${vehicleId}/images/${imageId}`);
  }

  private toParams(): HttpParams {
    const f = this.filters();
    let params = new HttpParams().set('page', this.page()).set('pageSize', this.pageSize());
    if (f.search) params = params.set('search', f.search);
    if (f.status) params = params.set('status', f.status);
    if (f.yearFrom != null) params = params.set('yearFrom', f.yearFrom);
    if (f.yearTo != null) params = params.set('yearTo', f.yearTo);
    if (f.priceFrom != null) params = params.set('priceFrom', f.priceFrom);
    if (f.priceTo != null) params = params.set('priceTo', f.priceTo);
    return params;
  }
}
