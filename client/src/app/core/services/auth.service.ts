import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

const API_BASE = 'http://localhost:5286';

export interface CurrentUser {
  name: string;
  claims: { type: string; value: string }[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _user = signal<CurrentUser | null>(null);
  private readonly _isLoading = signal(true);

  readonly user = this._user.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  constructor() {
    this.refresh();
  }

  async refresh(): Promise<void> {
    this._isLoading.set(true);
    try {
      const user = await firstValueFrom(
        this.http.get<CurrentUser>(`${API_BASE}/auth/me`, { withCredentials: true })
      );
      this._user.set(user);
    } catch {
      this._user.set(null);
    } finally {
      this._isLoading.set(false);
    }
  }

  loginWithKeycloak(): void {
    const returnUrl = encodeURIComponent(window.location.href);
    window.location.href = `${API_BASE}/auth/login?returnUrl=${returnUrl}`;
  }

  logout(): void {
    window.location.href = `${API_BASE}/auth/logout`;
  }

}
