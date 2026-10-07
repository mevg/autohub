export interface Branding {
  appName: string;
  logoUrl: string;
  faviconUrl: string;
  /** Debe coincidir con `BannerUrls` del DTO del backend (camelCase). */
  bannerUrls: string[];
  primaryLight: string;
  primary: string;
  primaryDark: string;
}

/** Valores por defecto: turquesa medio como color principal. */
export const DEFAULT_BRANDING: Branding = {
  appName: 'AutoHub',
  logoUrl: '',
  faviconUrl: 'favicon.ico',
  bannerUrls: [],
  primaryLight: '#71E2DD',
  primary: '#4DDBC4',
  primaryDark: '#45C5B0',
};
