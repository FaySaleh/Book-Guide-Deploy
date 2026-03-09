import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { AuthHeaderComponent } from '../../components/auth-header/auth-header';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AuthHeaderComponent],
  templateUrl: './forgot-password.html',
  styleUrls: ['./forgot-password.css']
})
export class ForgotPasswordComponent {
  email = '';
  loading = false;
  message = '';
  error = '';

  constructor(
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  submit() {
    this.error = '';
    this.message = '';

    const email = this.email.trim();
    if (!email) {
      this.error = 'Please enter your email.';
      return;
    }

    this.loading = true;

    this.auth.forgotPassword(email)
      .pipe(
        finalize(() => {
          this.loading = false;

          // ✅ مهم: يجبر الواجهة تتحدث حتى لو عندك مشكلة change detection
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (res: any) => {
          this.message = res?.message ?? 'If the email exists, a reset link will be sent.';
        },
        error: (err: any) => {
          this.message = 'If the email exists, a reset link will be sent.';

          console.error('Forgot password error:', err);
        }
      });
  }
}
