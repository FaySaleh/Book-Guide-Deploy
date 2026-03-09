import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserBook {
  id: number;
  userId: number;
  externalBookId: string;

  title?: string | null;
  author?: string | null;
  coverUrl?: string | null;

  status: number; 
  rating?: number | null;

  createdAt?: string | null;

  startedAt?: string | null;
  finishedAt?: string | null;
  currentPage?: number | null;
  totalPages?: number | null;
  lastProgressAt?: string | null;
}

export interface AddUserBookRequest {
  userId: number;
  externalBookId: string;
  title?: string | null;
  author?: string | null;
  coverUrl?: string | null;
  status: number;        
  rating?: number | null; 
}

export interface UpdateUserBookRequest {
  status: number;         
  rating?: number | null; 
}

export interface UserBookProgress {
  id: number;
  userId: number;
  title?: string | null;
  status: number;

  startedAt?: string | null;
  finishedAt?: string | null;
  currentPage?: number | null;
  totalPages?: number | null;
  lastProgressAt?: string | null;
}

export interface UpdateProgressRequest {
  currentPage?: number | null;
  totalPages?: number | null;
  startedAt?: string | null;
  finishedAt?: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserBooksService {
  constructor(private http: HttpClient) {}

  getUserBooks(userId: number, status?: number): Observable<UserBook[]> {
    let params = new HttpParams().set('userId', userId);
    if (status != null) params = params.set('status', status);
    return this.http.get<UserBook[]>('/api/UserBooks', { params });
  }

  add(req: AddUserBookRequest): Observable<UserBook> {
    return this.http.post<UserBook>('/api/UserBooks', req);
  }

  update(id: number, req: UpdateUserBookRequest): Observable<void> {
    return this.http.put<void>(`/api/UserBooks/${id}`, req);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/UserBooks/${id}`);
  }

  getProgress(id: number): Observable<UserBookProgress> {
    return this.http.get<UserBookProgress>(`/api/UserBooks/${id}/progress`);
  }

  updateProgress(id: number, req: UpdateProgressRequest): Observable<UserBookProgress> {
    return this.http.put<UserBookProgress>(`/api/UserBooks/${id}/progress`, req);
  }
}
