import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { VehicleDetailPage } from './vehicle-detail-page';
import { VehicleDetail } from '../vehicle.models';

const DETAIL: VehicleDetail = {
  id: 'abc',
  vin: '1HGCM82633A123456',
  make: 'Toyota',
  model: 'Corolla',
  year: 2022,
  price: 250000,
  mileage: 15000,
  transmission: 'Automática',
  fuelType: 'Gasolina',
  status: 'Available',
  description: 'Seminuevo',
  createdBy: 'u1',
  creatorEmail: 'vendedor@autohub.local',
  images: [],
};

describe('VehicleDetailPage', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleDetailPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function create(): ComponentFixture<VehicleDetailPage> {
    const fixture = TestBed.createComponent(VehicleDetailPage);
    return fixture;
  }

  it('renders detail with specs', () => {
    const fixture = create();
    httpMock.expectOne('/api/vehicles/').flush(DETAIL);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('h1')?.textContent).toContain('Toyota Corolla 2022');
    expect(el.textContent).toContain('vendedor@autohub.local');
    expect(el.textContent).toContain('Disponible');
  });

  it('shows not-found state on 404', () => {
    const fixture = create();
    httpMock.expectOne('/api/vehicles/').flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('no encontrado');
  });
});
