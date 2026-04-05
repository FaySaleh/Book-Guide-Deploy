import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';


export interface DashboardResponse {
  stats: {
    totalBooks: number;
    toRead: number;
    reading: number;
    finished: number;
    totalPagesRead: number;
    totalReadingDays: number;
    currentStreakDays: number;
  };
  achievements: Achievement[];
}

export interface Achievement {
  code: string;
  title: string;
  description: string;
  icon?: string;
  unlocked: boolean;
  unlockedAt?: string | null;
  targetValue?: number;
  currentValue: number;
  progressPercent: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
private api = `${environment.apiBaseUrl}/dashboard`;
  constructor(private http: HttpClient) {}

  getDashboard(userId: number): Observable<DashboardResponse> {
    return this.http.get<DashboardResponse>(`${this.api}/${userId}`);
  }
}