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

  totalPages?: number | null;
  currentPage?: number | null;
  progressPercent?: number | null;

  startedAt?: string | null;
  finishedAt?: string | null;
  lastReadAt?: string | null;
  lastProgressAt?: string | null;
}

export interface UserBookProgress {
  userBookId: number;
  currentPage: number | null;
  totalPages: number | null;
  progressPercent: number | null;
  startedAt: string | null;
  finishedAt: string | null;
  lastReadAt: string | null;
  lastProgressAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserBooksService {
  private apiBase = 'https://bookguide-api.onrender.com';

  constructor(private http: HttpClient) {}

  getUserBooks(userId: number): Observable<UserBook[]> {
    return this.http.get<UserBook[]>(`${this.apiBase}/api/UserBooks?userId=${userId}`);
  }

  getById(id: number): Observable<UserBook> {
    return this.http.get<UserBook>(`${this.apiBase}/api/UserBooks/${id}`);
  }

  add(data: {
    userId: number;
    externalBookId: string;
    title: string;
    author?: string | null;
    coverUrl?: string | null;
    status: number;
    rating?: number | null;
    totalPages?: number | null;
    currentPage?: number | null;
  }): Observable<any> {
    return this.http.post(`${this.apiBase}/api/UserBooks`, data);
  }

  update(id: number, data: Partial<UserBook>): Observable<any> {
    return this.http.put(`${this.apiBase}/api/UserBooks/${id}`, data);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiBase}/api/UserBooks/${id}`);
  }

  getProgress(id: number): Observable<UserBookProgress> {
    return this.http.get<UserBookProgress>(`${this.apiBase}/api/UserBooks/${id}/progress`);
  }

  updateProgress(id: number, data: {
    currentPage?: number | null;
    totalPages?: number | null;
  }): Observable<UserBookProgress> {
    return this.http.put<UserBookProgress>(
      `${this.apiBase}/api/UserBooks/${id}/progress`,
      data
    );
  }
}