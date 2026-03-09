import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { NotificationsService } from '../../services/notifications.service';
import { AuthService } from '../../services/auth.service';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './notifications.html',
  styleUrls: ['./notifications.css']
})
export class NotificationsComponent implements OnInit {

  items: any[] = [];
  loading = false;
  error = '';

  page = 1;
  pageSize = 20;
  total = 0;
  onlyUnread = false;
  userId = 0;

  constructor(
    private notifications: NotificationsService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef,
    private zone: NgZone,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.auth.currentUser;
    this.userId = user?.id ?? 0;

    if (!this.userId) {
      this.error = 'User not logged in.';
      this.cdr.detectChanges();
      return;
    }

    this.load();
  }

  load(): void {
    this.zone.run(() => {
      this.loading = true;
      this.error = '';
      this.cdr.detectChanges();
    });

    this.notifications
      .getMyNotifications(this.userId, this.page, this.pageSize, this.onlyUnread)
      .pipe(finalize(() => {
        this.zone.run(() => {
          this.loading = false;
          this.cdr.detectChanges();
        });
      }))
      .subscribe({
        next: (res) => {
          this.zone.run(() => {
            this.items = [...(res.items ?? [])];
            this.total = res.total ?? 0;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error(err);
          this.zone.run(() => {
            this.error = 'Failed to load notifications';
            this.cdr.detectChanges();
          });
        }
      });
  }

  toggleUnread(): void {
    this.onlyUnread = !this.onlyUnread;
    this.load();
  }

  openNotification(n: any): void {
    if (!n.isRead) {
      this.notifications.markRead(n.id, this.userId).subscribe({
        next: () => {
          this.zone.run(() => {
            this.items = this.items.map(x => x.id === n.id ? { ...x, isRead: true } : x);
            this.cdr.detectChanges();
          });

          this.notifications.refreshUnreadCount(this.userId).subscribe();
        },
        error: (e) => console.error(e)
      });
    }

    if (n.userBookId) {
      this.router.navigate(['/books', n.userBookId]);
    }
  }

  markAsRead(n: any): void {
    if (n.isRead) return;

    this.notifications.markRead(n.id, this.userId).subscribe(() => {
      this.items = this.items.map(x => x.id === n.id ? { ...x, isRead: true } : x);
      this.cdr.detectChanges();
      this.notifications.refreshUnreadCount(this.userId).subscribe();
    });
  }

  markAll(): void {
    this.notifications.markAllRead(this.userId).subscribe(() => {
      this.items = this.items.map(x => ({ ...x, isRead: true }));
      this.cdr.detectChanges();
      this.notifications.refreshUnreadCount(this.userId).subscribe();
    });
  }
}
