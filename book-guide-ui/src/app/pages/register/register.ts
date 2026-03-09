import { Component, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { AuthHeaderComponent } from '../../components/auth-header/auth-header';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AuthHeaderComponent],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {

  fullName = '';
  email = '';
  password = '';

  loading = false;
  error = '';
  success = '';

  constructor(
    private auth: AuthService,
    private router: Router,
    private zone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  onSubmit(): void {
    if (this.loading) return;

    this.zone.run(() => {
      this.error = '';
      this.success = '';
      this.loading = true;
      this.cdr.detectChanges();
    });

    const payload = {
      fullName: this.fullName.trim(),
      email: this.email.trim(),
      password: this.password
    };

    if (!payload.fullName || !payload.email || !payload.password) {
      this.zone.run(() => {
        this.loading = false;
        this.error = 'Please fill in all fields.';
        this.cdr.detectChanges();
      });
      return;
    }

    this.auth.register(
  payload.fullName,
  payload.email,
  payload.password
)

      .pipe(finalize(() => {
        this.zone.run(() => {
          this.loading = false;
          this.cdr.detectChanges();
        });
      }))
      .subscribe({
        next: () => {
          this.zone.run(() => {
            this.success = 'Account created successfully. Redirecting to login...';
            this.cdr.detectChanges();
          });

          setTimeout(() => this.router.navigateByUrl('/login'), 700);
        },
        error: (err) => {
          console.error(err);

          this.zone.run(() => {
            if (err?.status === 409) {
              this.error = 'This email is already registered. Please login instead.';
            } else {
              const msg = err?.error?.message || err?.error;
              this.error = (typeof msg === 'string' && msg.trim())
                ? msg
                : 'Registration failed. Please try again.';
            }
            this.cdr.detectChanges();
          });
        }
      });
  }
}
