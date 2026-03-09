import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { BooksComponent } from './pages/books/books';
import { BookDetailsComponent } from './pages/book-details/book-details';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';
import { NotificationsComponent } from './pages/notifications/notifications';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password';
import { ResetPasswordComponent } from './pages/reset-password/reset-password';
import { DashboardComponent } from './pages/dashboard/dashboard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
{path: 'dashboard',component: DashboardComponent},
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
{ path: 'notifications', loadComponent: () => import('./pages/notifications/notifications').then(m => m.NotificationsComponent) },
  { path: 'books', component: BooksComponent, canActivate: [authGuard]},
  { path: 'books/:id', component: BookDetailsComponent , canActivate: [authGuard]},
  { path: '**', redirectTo: 'login' }
];
