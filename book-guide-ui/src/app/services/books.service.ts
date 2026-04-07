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

    return this.http.get<any>(`${this.apiBase}/Books/search`, { params }).pipe(
      map((res: any) => {
        const arr: any[] = Array.isArray(res?.docs) ? res.docs : [];

        return arr.map((x: any) => ({
          externalBookId: x.key?.replace('/works/', '') ?? '',
          title: x.title ?? 'Untitled',
          author: Array.isArray(x.author_name) ? x.author_name.join(', ') : 'Unknown author',
          coverUrl: x.cover_i
            ? `https://covers.openlibrary.org/b/id/${x.cover_i}-L.jpg`
            : null
        }));
      })
    );
  }
}