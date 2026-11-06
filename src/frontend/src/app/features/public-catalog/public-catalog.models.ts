export interface PublicVehicleCard {
  id: string;
  publicReference: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  imageCount: number;
  coverImageUrl: string | null;
}

export interface PublicVehicleImage {
  id: string;
  imageUrl: string;
  altText: string | null;
  displayOrder: number;
}

export interface PublicVehicleDetail {
  id: string;
  publicReference: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  transmission: string;
  fuelType: string;
  description: string | null;
  images: PublicVehicleImage[];
  whatsAppUrl: string | null;
}

export interface PublicFacetOption {
  value: string;
  count: number;
}

export interface PublicCatalogFacets {
  makes: PublicFacetOption[];
  models: PublicFacetOption[];
  minYear: number | null;
  maxYear: number | null;
  minPrice: number | null;
  maxPrice: number | null;
}

export interface PublicCatalogFilters {
  search?: string;
  make?: string;
  model?: string;
  yearFrom?: number;
  yearTo?: number;
  priceFrom?: number;
  priceTo?: number;
  sort: PublicCatalogSort;
  page: number;
  pageSize: number;
}

export type PublicCatalogSort = 'newest' | 'price_asc' | 'price_desc' | 'year_desc';

export interface PublicCatalogFilterValue {
  search: string;
  make: string;
  model: string;
  yearFrom: number | null;
  yearTo: number | null;
  priceFrom: number | null;
  priceTo: number | null;
  sort: PublicCatalogSort;
}

export interface PublicLeadValue {
  name: string;
  phone: string;
  email: string;
  message?: string;
  website?: string;
}

export interface PublicPagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export const DEFAULT_CATALOG_PAGE_SIZE = 12;

export const SORT_OPTIONS: { value: PublicCatalogSort; label: string }[] = [
  { value: 'newest', label: 'Más recientes' },
  { value: 'price_asc', label: 'Menor precio' },
  { value: 'price_desc', label: 'Mayor precio' },
  { value: 'year_desc', label: 'Año más reciente' },
];

export function emptyFilterValue(): PublicCatalogFilterValue {
  return {
    search: '',
    make: '',
    model: '',
    yearFrom: null,
    yearTo: null,
    priceFrom: null,
    priceTo: null,
    sort: 'newest',
  };
}
