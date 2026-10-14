import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { VehiclesService } from './vehicles.service';
import { VehicleListItem } from './vehicle.models';

const ITEM: VehicleListItem = {
  id: '1',
  vin: '1HGCM82633A123456',
  make: 'Toyota',
  model: 'Corolla',
  year: 2022,
  price: 250000,
  mileage: 15000,
  status: 'Available',
  imageCount: 2,
  coverImageUrl: null,
};

describe('VehiclesService', () => {
  let service: VehiclesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(VehiclesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('load fills items and total', () => {
    service.load();
    const req = httpMock.expectOne((r) => r.url === '/api/vehicles');
    expect(req.request.params.get('page')).toBe('1');
    req.flush({ items: [ITEM], page: 1, pageSize: 12, totalCount: 1 });

    expect(service.items()).toEqual([ITEM]);
    expect(service.totalCount()).toBe(1);
    expect(service.loading()).toBe(false);
  });

  it('applyFilters resets to page 1 and sends params', () => {
    service.applyFilters({ search: 'toy', status: 'Available' });
    const req = httpMock.expectOne((r) => r.url === '/api/vehicles');
    expect(req.request.params.get('search')).toBe('toy');
    expect(req.request.params.get('status')).toBe('Available');
    expect(req.request.params.get('page')).toBe('1');
    req.flush({ items: [], page: 1, pageSize: 12, totalCount: 0 });
    expect(service.page()).toBe(1);
  });

  it('sets error message on failure', () => {
    service.load();
    httpMock
      .expectOne((r) => r.url === '/api/vehicles')
      .flush(null, { status: 500, statusText: 'Error' });
    expect(service.error()).toContain('No se pudieron cargar');
    expect(service.loading()).toBe(false);
  });

  it('computes total pages', () => {
    service.load();
    httpMock
      .expectOne((r) => r.url === '/api/vehicles')
      .flush({ items: [], page: 1, pageSize: 12, totalCount: 25 });
    expect(service.totalPages()).toBe(3);
  });
});
