import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardResponse, Achievement } from '../../services/dashboard.service';
import { AuthService } from '../../services/auth.service';
import { ChangeDetectorRef } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { finalize } from 'rxjs/operators';

type StatCard = { label: string; value: number; icon: string; hint?: string };

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  data?: DashboardResponse;
  loading = true;
  error = '';

  cards: StatCard[] = [];
  showMore = false;

  get unlockedCount(): number {
    return this.data?.achievements?.filter(x => x.unlocked).length ?? 0;
  }

  get primaryCards() {
    return this.cards.slice(0, 4);
  }

  get extraCards() {
    return this.cards.slice(4);
  }

  constructor(
    private dashboard: DashboardService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const user = this.auth.currentUser;

    if (!user) {
      this.error = 'User not logged in';
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }

    this.error = '';
    this.loading = true;

    this.dashboard.getDashboard(user.id)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res) => {
          this.data = res;
          this.cards = [
            { label: 'DASH.STATS.TOTAL_BOOKS', value: res.stats.totalBooks, icon: '📚' },
            { label: 'DASH.STATS.FINISHED', value: res.stats.finished, icon: '✅' },
            { label: 'DASH.STATS.PAGES_READ', value: res.stats.totalPagesRead, icon: '📖' },
            { label: 'DASH.STATS.READING_DAYS', value: res.stats.totalReadingDays, icon: '📅' },
            { label: 'DASH.STATS.TO_READ', value: res.stats.toRead, icon: '⏳' },
            { label: 'DASH.STATS.READING_NOW', value: res.stats.reading, icon: '📝' },
            { label: 'DASH.STATS.STREAK', value: res.stats.currentStreakDays, icon: '🔥', hint: 'days' },
          ];
        },
        error: (err) => {
          console.error('DASHBOARD ERROR:', err);
          this.error =
            (typeof err?.error === 'string' ? err.error : '') || 'Failed to load dashboard';
        }
      });
  }

  toggleMore() {
    this.showMore = !this.showMore;
  }

  trackByCode(_: number, a: Achievement) {
    return a.code;
  }
}