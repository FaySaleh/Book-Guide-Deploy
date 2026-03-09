import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { NotificationsService } from '../../services/notifications.service';
import { LangService } from '../../services/lang.service';
import { TranslateModule } from '@ngx-translate/core';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent implements OnInit, OnDestroy {
  isLoggedIn = false;
  userName = '';
  unreadCount = 0;

  private subs = new Subscription();

  constructor(
    public  auth: AuthService,
    private notifications: NotificationsService,
    private router: Router,
    public lang: LangService,
    private cdr: ChangeDetectorRef

  ) {}

  toggleLang() {
  this.lang.toggle();
  this.cdr.detectChanges();
}
  ngOnInit(): void {
    this.subs.add(
      this.notifications.unreadCount$.subscribe(n => this.unreadCount = n ?? 0)
    );

    this.subs.add(
      this.auth.user$.subscribe(user => {
        this.isLoggedIn = !!user?.id;
        this.userName = user?.fullName ?? '';

        if (this.isLoggedIn) {
          const userId = user!.id;
          this.notifications.refreshUnreadCount(userId).subscribe({
            error: () => this.notifications.setUnreadCount(0)
          });
        } else {
          this.notifications.setUnreadCount(0);
        }
      })
    );
  }

  logout(): void {
    this.auth.logout();
    this.notifications.setUnreadCount(0);
    this.router.navigateByUrl('/login');
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  
}
