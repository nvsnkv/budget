import { inject, provideAppInitializer } from '@angular/core';
import { AppConfigService } from './app-config.service';
import { AuthService } from '../auth/auth/auth.service';

export function initializeApp(
  configService: AppConfigService,
  authService: AuthService
): Promise<void> {
  return configService.loadConfig().then(() => {
    // Set the API URL in AuthService after config is loaded
    authService.setBaseUrl(configService.apiUrl);
  });
}

export const appConfigInitializerProvider = provideAppInitializer(() => {
  const configService = inject(AppConfigService);
  const authService = inject(AuthService);
  return initializeApp(configService, authService);
});
