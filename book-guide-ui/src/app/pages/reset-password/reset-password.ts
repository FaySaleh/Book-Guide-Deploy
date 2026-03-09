import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { AuthHeaderComponent } from '../../components/auth-header/auth-header';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AuthHeaderComponent],
  templateUrl: './reset-password.html',
  styleUrls: ['./reset-password.css']
})
export class ResetPasswordComponent {
  token = '';
  newPassword = '';
  confirmPassword = '';

  loading = false;
  message = '';
  error = '';

  constructor(
    private route: ActivatedRoute,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
  }

  submit() {
    this.error = '';
    this.message = '';

    if (!this.token) {
      this.error = 'Invalid or missing reset token.';
      return;
    }

    const p1 = this.newPassword.trim();
    const p2 = this.confirmPassword.trim();

    if (!p1 || !p2) {
      this.error = 'Please enter and confirm your new password.';
      return;
    }

    if (p1.length < 8) {
      this.error = 'Password must be at least 8 characters.';
      return;
    }

    if (p1 !== p2) {
      this.error = 'Passwords do not match.';
      return;
    }

    this.loading = true;

    this.auth.resetPassword(this.token, p1)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any) => {
          this.message = res?.message ?? 'Password updated successfully. You can log in now.';
        },
        error: (err: any) => {
          this.error = err?.error?.message ?? 'Failed to reset password. Please try again.';
          console.error('Reset password error:', err);
        }
      });
  }
}
