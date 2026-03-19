import { ApplicationConfig, LOCALE_ID, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

import { routes } from './app.routes';
import { registerLocaleData } from '@angular/common';
import localeIn from '@angular/common/locales/en-IN';

// Register the locale data for India
registerLocaleData(localeIn, 'en-IN');

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideCharts(withDefaultRegisterables()),
    { provide: LOCALE_ID, useValue: 'en-IN' }
  ]
};
