import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { AuthHeaderComponent } from '../../components/auth-header/auth-header';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AuthHeaderComponent],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})

export class LoginComponent {
  email = '';
  password = '';

  loading = false;
  error = '';

  constructor(
    private auth: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  onSubmit() {
    this.error = '';

    if (!this.email.trim() || !this.password) {
      this.error = 'Please enter email and password';
      this.cdr.detectChanges();
      return;
    }

    this.loading = true;
    this.cdr.detectChanges();

    this.auth
      .login(this.email.trim(), this.password)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.router.navigateByUrl('/books');
        },
        error: (err) => {
          console.log('LOGIN ERROR =>', err);

          if (typeof err?.error === 'string' && err.error.trim()) {
            this.error = err.error.trim();
          }
          else if (err?.error?.message) {
            this.error = String(err.error.message);
          }
          else if (err?.status === 401) {
            this.error = 'Invalid email or password';
          } else {
            this.error = 'Login failed';
          }

          this.cdr.detectChanges();
        }
      });
  }
}
