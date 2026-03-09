import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface ExternalBook {
  externalBookId: string;
  title: string;
  author?: string;
  coverUrl?: string;   

}

@Injectable({ providedIn: 'root' })
export class BooksService {
  constructor(private http: HttpClient) {}

  search(title: string): Observable<ExternalBook[]> {
    const params = new HttpParams().set('title', title); 

    return this.http.get<any[]>('/api/Books/search', { params }).pipe(
      map(arr => (arr ?? []).map(x => ({
        externalBookId: x.externalBookId ?? x.externalBookID ?? '',
        title: x.title ?? '',
        author: x.author ?? '',
        coverUrl: x.coverUrl ?? null    
      })))
    );
  }
}