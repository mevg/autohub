import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { VehiclesPage } from './vehicles-page';
import { VehiclesService } from '../vehicles.service';
import { VehicleListItem } from '../vehicle.models';

const ITEM: VehicleListItem = {
  id: 'abc',
  vin: '1HGCM82633A123456',
  make: 'Toyota',
  model: 'Corolla',
  year: 2022,
  price: 250000,
  mileage: 15000,
  status: 'Available',
  imageCount: 0,
};

const EMPTY_PAGE = { items: [], page: 1, pageSize: 12, totalCount: 0 };

describe('VehiclesPage', () => {
  let fixture: ComponentFixture<VehiclesPage>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehiclesPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(VehiclesPage);
    // El constructor dispara la carga inicial.
    httpMock.expectOne((r) => r.url === '/api/vehicles').flush(EMPTY_PAGE);
  });

  afterEach(() => httpMock.verify());

  it('shows skeletons while loading', () => {
    TestBed.inject(VehiclesService).loading.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-vehicle-card-skeleton').length).toBe(6);
  });

  it('shows empty state without items', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Sin vehículos');
  });

  it('renders cards and opens dialog', () => {
    const service = TestBed.inject(VehiclesService);
    service.loading.set(false);
    service.items.set([ITEM]);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('Toyota Corolla');
    expect(el.textContent).toContain('Disponible');

    (el.querySelector('button') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(el.querySelector('app-vehicle-form-dialog')).toBeTruthy();
  });
});
