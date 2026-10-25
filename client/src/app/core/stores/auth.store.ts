import { computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';

const API = 'http://localhost:5286';

export interface AuthUser {
  id: string;
  username: string;
  roles: string[];
}

const APP_ROLES = ['analyst', 'developer', 'pm', 'auditor']; // put this above the signalStore

interface AuthState {
  user: AuthUser | null;
}

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState<AuthState>({ user: null }),
  withComputed(({ user }) => ({
    isAuthenticated: computed(() => user() !== null),
  })),
  withMethods((store, http = inject(HttpClient)) => ({
    // asks the API who is logged in (the cookie is sent because of withCredentials)


    async loadUser(): Promise<void> {
      try {
        const user = await firstValueFrom(
          http.get<AuthUser>(`${API}/auth/me`, { withCredentials: true }),
        );
        // drop Keycloak's built-in roles, keep only the app roles
        patchState(store, { user: { ...user, roles: user.roles.filter((r) => APP_ROLES.includes(r)) } });
      } catch {
        patchState(store, { user: null });
      }
    },

    hasRole(role: string): boolean {
      return store.user()?.roles.includes(role) ?? false;
    },

    // full page redirect: Angular -> API /auth/login -> Keycloak -> back to /home
    loginWithKeycloak(): void {
      const back = encodeURIComponent(`${window.location.origin}/home`);
      window.location.href = `${API}/auth/login?returnUrl=${back}`;
    },

    logout(): void {
      window.location.href = `${API}/auth/logout`;
    },
  })),
);
