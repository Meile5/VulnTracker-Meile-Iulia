import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';

interface AuthState {
  isAuthenticated: boolean;
  userEmail: string | null;
  loading: boolean;
}

const initialState: AuthState = {
  isAuthenticated: false,
  userEmail: null,
  loading: false,
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => ({
    async login(email: string, _password: string): Promise<void> {
      patchState(store, { loading: true });

      await new Promise((resolve) => setTimeout(resolve, 500));

      patchState(store, {
        isAuthenticated: true,
        userEmail: email,
        loading: false,
      });
    },

    loginWithKeycloak(): void {
      console.log('Keycloak login not wired up yet');
    },

    logout(): void {
      patchState(store, initialState);
    },
  })),
);
