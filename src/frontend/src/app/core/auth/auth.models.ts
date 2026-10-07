/** Coincide con AuthResponseDto del backend (camelCase en JSON). */
export interface AuthResponse {
  id: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface StoredSession {
  token: string;
  user: CurrentUser;
}
