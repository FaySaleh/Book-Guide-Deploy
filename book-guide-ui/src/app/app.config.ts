import { ApplicationConfig, importProvidersFrom, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, HttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { Observable } from 'rxjs';

import { LangService } from './services/lang.service';

class PublicTranslateLoader implements TranslateLoader {
  constructor(private http: HttpClient) {}

  getTranslation(lang: string): Observable<any> {
    return this.http.get(`/i18n/${lang}.json`);
  }
}

export function translateLoaderFactory(http: HttpClient) {
  return new PublicTranslateLoader(http);
}

// ✅ يضمن أن lang.init() يشتغل قبل تشغيل التطبيق
export function langInitFactory(lang: LangService) {
  return () => lang.init();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),

    {
      provide: APP_INITIALIZER,
      useFactory: langInitFactory,
      deps: [LangService],
      multi: true
    },

    importProvidersFrom(
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: translateLoaderFactory,
          deps: [HttpClient]
        }
      })
    )
  ]
};