import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class LangService {
  private readonly KEY = 'bg_lang';

  constructor(private translate: TranslateService) {}

  init() {
    const saved = localStorage.getItem(this.KEY);
    const lang: 'en' | 'ar' = saved === 'ar' ? 'ar' : 'en';

    this.translate.addLangs(['en', 'ar']);
    this.translate.setDefaultLang('en');
    this.use(lang);
  }

  current(): 'en' | 'ar' {
    const l = (this.translate.currentLang || this.translate.defaultLang || 'en');
    return l === 'ar' ? 'ar' : 'en';
  }

  toggle() {
    const next: 'en' | 'ar' = this.current() === 'ar' ? 'en' : 'ar';
    this.use(next);
  }

  private use(lang: 'en' | 'ar') {
    this.translate.use(lang);
    localStorage.setItem(this.KEY, lang);

    document.documentElement.setAttribute('lang', lang);
    document.documentElement.setAttribute('dir', 'ltr'); // مؤقتًا بدون RTL
  }
}