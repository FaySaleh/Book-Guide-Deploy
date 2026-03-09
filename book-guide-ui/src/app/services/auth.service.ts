import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface AuthUser {
  id: number;
  fullName: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly STORAGE_KEY = 'bg_user';
  private userSubject = new BehaviorSubject<AuthUser | null>(this.loadUser());

  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {}

  get currentUser(): AuthUser | null {
    return this.userSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.currentUser?.id;
  }

  register(fullName: string, email: string, password: string): Observable<AuthUser> {
    return this.http
      .post<AuthUser>('/api/Auth/register', { fullName, email, password })
      .pipe(
        tap(user => {
          this.saveUser(user);
        })
      );
  }

  login(email: string, password: string): Observable<AuthUser> {
    return this.http
      .post<AuthUser>('/api/Auth/login', { email, password })
      .pipe(tap(user => this.saveUser(user)));
  }

    forgotPassword(email: string): Observable<any> {
    return this.http.post<any>('/api/Auth/forgot-password', { email });
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post<any>('/api/Auth/reset-password', { token, newPassword });
  }


  logout(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.userSubject.next(null);
  }
getUserId(): number {
  return this.currentUser?.id ?? 0;
}


  private saveUser(user: AuthUser): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
    this.userSubject.next(user);
  }

  private loadUser(): AuthUser | null {
    try {
      const raw = localStorage.getItem(this.STORAGE_KEY);
      return raw ? (JSON.parse(raw) as AuthUser) : null;
    } catch {
      return null;
    }
  }
}

