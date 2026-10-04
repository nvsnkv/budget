import { provideTaiga } from "@taiga-ui/core";
import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';
import { appConfigInitializerProvider } from './config/app-config.initializer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withXhr(), withInterceptorsFromDi()),
    provideTaiga(),
    appConfigInitializerProvider
  ]
};
