export interface AuthResponse {
  token: string;
  id: string;
  name: string;
  email: string;
}

export interface RegisterCustomerRequest {
  name: string;
  email: string;
  password: string;
  phone: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthUser {
  id: string;
  name: string;
  email: string;
}
