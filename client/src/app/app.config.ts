import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { routes } from './app.routes';
import { AuthStore } from './core/stores/auth.store';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(), // AuthStore needs HttpClient
    // asks the API who is logged in before any page shows, so the guard knows the answer
    provideAppInitializer(() => inject(AuthStore).loadUser()),
    providePrimeNG({ theme: { preset: Aura } }),
  ],
};
