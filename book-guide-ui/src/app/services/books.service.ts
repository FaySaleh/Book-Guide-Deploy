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
  constructor(private http: HttpClient) {}

  search(title: string): Observable<ExternalBook[]> {
    const params = new HttpParams().set('title', title.trim());

    return this.http.get<any>('https://localhost:7250/api/Books/search', { params }).pipe(
      map((res: any) => {
        console.log('Books search raw response:', res);

        let arr: any[] = [];

        if (Array.isArray(res)) {
          arr = res;
        } else if (res && Array.isArray(res.docs)) {
          arr = res.docs;
        } else if (res && Array.isArray(res.items)) {
          arr = res.items;
        }

        return arr.map((x: any) => ({
          externalBookId:
            x.externalBookId ??
            x.externalBookID ??
            x.ExternalBookId ??
            x.ExternalBookID ??
            x.id ??
            x.Id ??
            x.key ??
            x.Key ??
            '',

          title:
            x.title ??
            x.Title ??
            x.name ??
            x.Name ??
            'Untitled',

          author:
            x.author ??
            x.Author ??
            x.authorName ??
            x.AuthorName ??
            (Array.isArray(x.author_name) ? x.author_name.join(', ') : x.author_name) ??
            'Unknown author',

          coverUrl:
            x.coverUrl ??
            x.CoverUrl ??
            x.cover ??
            x.Cover ??
            (x.cover_i ? `https://covers.openlibrary.org/b/id/${x.cover_i}-M.jpg` : null)
        }));
      })
    );
  }
}