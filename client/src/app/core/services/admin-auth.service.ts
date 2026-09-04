import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, AuthUser, LoginRequest } from '../models/auth.model';

const STORAGE_KEY = 'atelie-bebe.admin.token';
const USER_KEY = 'atelie-bebe.admin.user';

@Injectable({ providedIn: 'root' })
export class AdminAuthService {
  private readonly userSignal = signal<AuthUser | null>(this.readStoredUser());

  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.userSignal() !== null);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/admin/auth/login`, request)
      .pipe(
        tap((response) => {
          localStorage.setItem(STORAGE_KEY, response.token);
          const user: AuthUser = { id: response.id, name: response.name, email: response.email };
          localStorage.setItem(USER_KEY, JSON.stringify(user));
          this.userSignal.set(user);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem(USER_KEY);
    this.userSignal.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEY);
  }

  private readStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as AuthUser) : null;
  }
}
