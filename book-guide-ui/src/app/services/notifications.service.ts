import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface NotificationDto {
  id: number;
  userId: number;
  userBookId?: number | null;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface PagedNotifications {
  page: number;
  pageSize: number;
  total: number;
  items: NotificationDto[];
}

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private apiBase = `${environment.apiBaseUrl}/Notifications`;
  private unreadSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadSubject.asObservable();

  constructor(private http: HttpClient) {}

  setUnreadCount(count: number) {
    this.unreadSubject.next(Number(count ?? 0));
  }

  refreshUnreadCount(userId: number): Observable<{ userId: number; unread: number }> {
    const params = new HttpParams().set('userId', userId);
    return this.http
      .get<{ userId: number; unread: number }>(`${this.apiBase}/unread-count`, { params })
      .pipe(tap(res => this.setUnreadCount(res.unread)));
  }

  getMyNotifications(
    userId: number,
    page = 1,
    pageSize = 20,
    onlyUnread = false
  ): Observable<PagedNotifications> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('page', page)
      .set('pageSize', pageSize)
      .set('onlyUnread', onlyUnread);

    return this.http.get<PagedNotifications>(this.apiBase, { params });
  }

  markRead(id: number, userId: number): Observable<void> {
    const params = new HttpParams().set('userId', userId);
    return this.http.put<void>(`${this.apiBase}/${id}/read`, {}, { params });
  }

  markAllRead(userId: number): Observable<{ updated: number }> {
    const params = new HttpParams().set('userId', userId);
    return this.http.put<{ updated: number }>(`${this.apiBase}/mark-all-read`, {}, { params });
  }

  runReminders(days = 3, userId?: number): Observable<any> {
    let params = new HttpParams().set('days', days);
    if (userId) params = params.set('userId', userId);
    return this.http.post<any>(`${this.apiBase}/run-reminders`, {}, { params });
  }
}