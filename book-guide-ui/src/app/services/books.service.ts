import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';


export interface ExternalBook {
  externalBookId: string;
  title: string;
  author?: string;
  coverUrl?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BooksService {
private apiBase = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

search(title: string): Observable<ExternalBook[]> {
  const cleanTitle = title.trim();
  const params = new HttpParams().set('title', cleanTitle);

  console.log('SEARCH URL:', `${this.apiBase}/api/Books/search`);
  console.log('SEARCH TITLE:', cleanTitle);

  return this.http.get<any>(`${this.apiBase}/api/Books/search`, { params }).pipe(
    map((res: any) => {
      console.log('RAW SEARCH RESPONSE:', res);

      const arr: any[] = Array.isArray(res) ? res : [];

      return arr.map((x: any) => ({
        externalBookId:
          x.externalBookId ??
          x.ExternalBookId ??
          '',

        title:
          x.title ??
          x.Title ??
          'Untitled',

        author:
          x.author ??
          x.Author ??
          'Unknown author',

        coverUrl:
          x.coverUrl ??
          x.CoverUrl ??
          null
      }));
    })
  );
}
}