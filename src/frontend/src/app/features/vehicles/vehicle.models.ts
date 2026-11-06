/** Coincide con los DTOs del backend (camelCase en JSON). */
export interface VehicleImage {
  id: string;
  imageUrl: string;
  displayOrder: number;
}

export interface VehicleListItem {
  id: string;
  vin: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  status: string;
  imageCount: number;
  coverImageUrl: string | null;
}

export interface VehicleDetail extends Omit<VehicleListItem, 'imageCount' | 'coverImageUrl'> {
  transmission: string;
  fuelType: string;
  description: string | null;
  createdBy: string;
  creatorEmail: string;
  images: VehicleImage[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface VehicleFilters {
  search?: string;
  status?: string;
  yearFrom?: number;
  yearTo?: number;
  priceFrom?: number;
  priceTo?: number;
  page: number;
  pageSize: number;
}

export interface VehicleFormValue {
  vin: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  transmission: string;
  fuelType: string;
  status?: string;
  description?: string | null;
}

export const VEHICLE_STATUSES = ['Available', 'Reserved', 'Sold', 'Archived'] as const;

export const STATUS_LABELS: Record<string, string> = {
  Available: 'Disponible',
  Reserved: 'Apartado',
  Sold: 'Vendido',
  Archived: 'Archivado',
};

export const STATUS_STYLES: Record<string, string> = {
  Available: 'bg-brand-green/15 text-brand-green-dark',
  Reserved: 'bg-brand-yellow/20 text-brand-yellow-dark',
  Sold: 'bg-brand-blue/15 text-brand-blue-dark',
  Archived: 'bg-brand-gray/15 text-brand-gray-dark',
};

export function formatPrice(value: number): string {
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
    maximumFractionDigits: 0,
  }).format(value);
}

export function formatMileage(value: number): string {
  return `${new Intl.NumberFormat('es-MX').format(value)} km`;
}
