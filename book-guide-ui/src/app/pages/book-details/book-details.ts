import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserBooksService, UserBook, UserBookProgress } from '../../services/userbooks.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-book-details',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './book-details.html',
  styleUrls: ['./book-details.css']
})
export class BookDetailsComponent {
  id = 0;
  title = '';
  author = '';
  description = '';
  coverUrl = '';

  status = 1;
  rating: number | null = null;

  stars = [1, 2, 3, 4, 5];

  progress: UserBookProgress | null = null;
  currentPageInput: number | null = null;
  totalPagesInput: number | null = null;

  loadingProgress = false;
  progressError = '';
  progressSuccess = '';

  constructor(
    private route: ActivatedRoute,
    private auth: AuthService,
    private userBooksApi: UserBooksService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.id = Number(this.route.snapshot.paramMap.get('id') || 0);
    this.loadBookDetailsFromLibrary();
    this.loadProgress();
  }

  statusLabel(s: number) {
    if (s === 1) return 'To Read';
    if (s === 2) return 'Reading';
    return 'Finished';
  }

  get progressPercent(): number {
    const c = this.progress?.currentPage ?? 0;
    const t = this.progress?.totalPages ?? 0;

    if (!t || t <= 0) return 0;

    const pct = Math.round((c / t) * 100);
    return Math.max(0, Math.min(100, pct));
  }

  onCoverError(ev: Event) {
    const img = ev.target as HTMLImageElement;
    img.src = '/placeholder-book.png';
  }

  loadBookDetailsFromLibrary() {
    const user = this.auth.currentUser;
    if (!user) return;

    this.userBooksApi.getUserBooks(user.id).subscribe({
      next: (list: UserBook[]) => {
        const found = (list ?? []).find((x: UserBook) => x.id === this.id);
        if (!found) return;

        this.applyUserBook(found);
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('LOAD BOOK DETAILS ERROR:', err);
      }
    });
  }

  private applyUserBook(b: UserBook) {
    this.title = b.title ?? '';
    this.author = b.author ?? '';
    this.coverUrl = b.coverUrl ?? '';

    this.status = b.status ?? 1;
    this.rating = b.rating ?? null;

    if (b.currentPage != null || b.totalPages != null) {
      this.progress = {
        id: b.id,
        userBookId: b.id,
        status: b.status ?? 1,
        currentPage: b.currentPage ?? null,
        totalPages: b.totalPages ?? null,
        progressPercent: b.progressPercent ?? null,
        startedAt: b.startedAt ?? null,
        finishedAt: b.finishedAt ?? null,
        lastReadAt: b.lastReadAt ?? null,
        lastProgressAt: b.lastProgressAt ?? null
      };

      this.currentPageInput = this.progress?.currentPage ?? 0;
      this.totalPagesInput = this.progress?.totalPages ?? null;
    }
  }

  setStatus(s: number) {
    const req = { status: s, rating: this.rating };

    this.userBooksApi.update(this.id, req).subscribe({
      next: () => {
        this.status = s;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('SET STATUS ERROR:', err);
      }
    });
  }

  setRating(r: number) {
    const newRating = this.rating === r ? null : r;
    const req = { status: this.status, rating: newRating };

    this.userBooksApi.update(this.id, req).subscribe({
      next: () => {
        this.rating = newRating;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('SET RATING ERROR:', err);
      }
    });
  }

  loadProgress() {
    this.loadingProgress = true;
    this.progressError = '';
    this.progressSuccess = '';

    this.userBooksApi.getProgress(this.id).subscribe({
      next: (p: UserBookProgress) => {
        this.loadingProgress = false;
        this.progress = p;

        this.status = p.status ?? this.status;
        this.currentPageInput = p.currentPage ?? 0;
        this.totalPagesInput = p.totalPages ?? null;

        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.loadingProgress = false;
        this.progressError =
          (typeof err?.error === 'string' ? err.error : '') || 'Failed to load progress';
        this.cdr.detectChanges();
      }
    });
  }

  saveProgress() {
    this.progressError = '';
    this.progressSuccess = '';

    const current = this.currentPageInput ?? 0;
    const total = this.totalPagesInput ?? null;

    if (total == null || total <= 0) {
      this.progressError = 'Total pages must be greater than 0.';
      this.cdr.detectChanges();
      return;
    }

    if (current < 0) {
      this.progressError = 'Current page must be 0 or greater.';
      this.cdr.detectChanges();
      return;
    }

    if (current > total) {
      this.progressError = 'Current page cannot be greater than total pages.';
      this.cdr.detectChanges();
      return;
    }

    this.loadingProgress = true;

    this.userBooksApi.updateProgress(this.id, {
      currentPage: current,
      totalPages: total
    }).subscribe({
      next: (p: UserBookProgress) => {
        this.loadingProgress = false;
        this.progress = p;

        this.status = p.status ?? this.status;
        this.currentPageInput = p.currentPage ?? current;
        this.totalPagesInput = p.totalPages ?? total;

        this.progressSuccess = 'Progress updated.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.loadingProgress = false;
        this.progressError =
          (typeof err?.error === 'string' ? err.error : '') || 'Failed to update progress';
        this.cdr.detectChanges();
      }
    });
  }
}