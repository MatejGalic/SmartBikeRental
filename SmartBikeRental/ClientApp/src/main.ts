import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return environment.baseUrl + '/api';
}

export function getHubUrl() {
  return environment.baseUrl + '/hub';
}

const providers = [
  { provide: 'API_URL', useFactory: getBaseUrl, deps: [] },
  { provide: 'HUB_URL', useFactory: getHubUrl, deps: [] },
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers)
  .bootstrapModule(AppModule)
  .catch((err) => console.log(err));
