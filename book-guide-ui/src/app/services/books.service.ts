import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface ExternalBook {
  externalBookId: string;
  title: string;
  author?: string;
  coverUrl?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BooksService {
  private apiBase = 'https://bookguide-api.onrender.com';

  constructor(private http: HttpClient) {}

  search(title: string): Observable<ExternalBook[]> {
    const params = new HttpParams().set('title', title.trim());

    return this.http.get<any>(`${this.apiBase}/api/Books/search`, { params }).pipe(
      map((res: any) => {
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