import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserBook {
  id: number;
  userId: number;
  externalBookId: string;
  title: string;
  author?: string | null;
  coverUrl?: string | null;
  status: number;
  rating?: number | null;
}

@Injectable({ providedIn: 'root' })
export class UserBooksService {
  private apiBase = 'https://bookguide-api.onrender.com';

  constructor(private http: HttpClient) {}

  getUserBooks(userId: number): Observable<UserBook[]> {
    return this.http.get<UserBook[]>(`${this.apiBase}/api/UserBooks?userId=${userId}`);
  }

  add(data: {
    userId: number;
    externalBookId: string;
    title: string;
    author?: string | null;
    coverUrl?: string | null;
    status: number;
    rating?: number | null;
  }): Observable<any> {
    return this.http.post(`${this.apiBase}/api/UserBooks`, data);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiBase}/api/UserBooks/${id}`);
  }
}