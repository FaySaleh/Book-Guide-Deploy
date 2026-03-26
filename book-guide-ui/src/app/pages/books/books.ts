import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BooksService, ExternalBook } from '../../services/books.service';
import { UserBooksService, UserBook } from '../../services/userbooks.service';
import { AuthService } from '../../services/auth.service';
import { TranslateModule } from '@ngx-translate/core';

type Filter = 'all' | 'ToRead' | 'Reading' | 'Finished';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule],
  templateUrl: './books.html',
  styleUrls: ['./books.css']
})
export class BooksComponent implements OnInit {
  activeFilter: Filter = 'all';
  filteredBooks: UserBook[] = [];
  myBooks: UserBook[] = [];

  loadingLibrary = false;
  libraryError = '';

  searchTerm = '';
  searching = false;
  results: ExternalBook[] = [];
  searchError = '';

  constructor(
    private booksApi: BooksService,
    private userBooksApi: UserBooksService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef,
    private zone: NgZone
  ) {}

  ngOnInit() {
    this.loadMyLibrary();
  }

  loadMyLibrary() {
    const user = this.auth.currentUser;
    if (!user) {
      this.libraryError = 'You must login first';
      this.filteredBooks = [];
      this.myBooks = [];
      this.cdr.detectChanges();
      return;
    }

    this.loadingLibrary = true;
    this.libraryError = '';

    this.userBooksApi.getUserBooks(user.id).subscribe({
      next: (list) => {
        this.loadingLibrary = false;
        this.myBooks = list ?? [];
        this.applyFilter();
        this.zone.run(() => this.cdr.detectChanges());
      },
      error: (err) => {
        this.loadingLibrary = false;
        this.libraryError =
          (typeof err?.error === 'string' ? err.error : '') || 'Failed to load library';
        this.zone.run(() => this.cdr.detectChanges());
      }
    });
  }

  setFilter(f: Filter) {
    this.activeFilter = f;
    this.applyFilter();
    this.cdr.detectChanges();
  }

  applyFilter() {
    if (this.activeFilter === 'all') {
      this.filteredBooks = this.myBooks;
      return;
    }

    const statusMap: Record<Exclude<Filter, 'all'>, number> = {
      ToRead: 1,
      Reading: 2,
      Finished: 3
    };

    const wanted = statusMap[this.activeFilter as Exclude<Filter, 'all'>];
    this.filteredBooks = this.myBooks.filter((b) => b.status === wanted);
  }

  statusLabel(status: number): string {
    if (status === 1) return 'To Read';
    if (status === 2) return 'Reading';
    if (status === 3) return 'Finished';
    return 'Unknown';
  }

  onCoverError(e: Event) {
    const img = e.target as HTMLImageElement;

    if (img.src.includes('placeholder-book.png')) return;

    img.src = '/placeholder-book.png';
  }

  searchExternal() {
    const term = (this.searchTerm ?? '').trim();
    if (!term) return;

    this.searching = true;
    this.searchError = '';
    this.results = [];

    this.cdr.detectChanges();

    this.booksApi
      .search(term)
      .pipe(
        finalize(() => {
          this.searching = false;
          this.zone.run(() => this.cdr.detectChanges());
        })
      )

      .subscribe({
next: (res) => {
  console.log('SEARCH RESULT AFTER MAPPING:', res);
  this.results = (res ?? []).filter(b => !!b.title && b.title.trim().length > 0);
  this.cdr.detectChanges();
},
error: (err) => {
  console.error('SEARCH ERROR:', err);
  this.searchError =
    (typeof err?.error === 'string' ? err.error : '') || 'Search failed';
  this.cdr.detectChanges();
}
      });
  }

  

  clearSearch() {
    this.searchTerm = '';
    this.results = [];
    this.searchError = '';
    this.cdr.detectChanges();
  }

addToMyLibrary(b: ExternalBook) {
  const user = this.auth.currentUser;
  if (!user) {
    alert('You must login first');
    return;
  }

  this.userBooksApi
    .add({
      userId: user.id,
      externalBookId: b.externalBookId,
      title: b.title ?? '',
      author: b.author ?? null,
      coverUrl: b.coverUrl ?? null,
      status: 1,
      rating: null
    })
    .subscribe({
      next: (res) => {
        console.log('ADD BOOK SUCCESS:', res);
        this.loadMyLibrary();
        alert('Added to your library');
      },
      error: (err) => {
        console.error('ADD BOOK ERROR:', err);
        alert((typeof err?.error === 'string' ? err.error : '') || 'Failed to add book');
      }
    });
}

  deleteFromMyLibrary(b: UserBook) {
    const id = (b as any).id;
    if (!id) return;

    const ok = confirm(`Delete "${b.title}" from your library?`);
    if (!ok) return;

    this.userBooksApi.delete(id).subscribe({
      next: () => {
        this.myBooks = this.myBooks.filter((x) => (x as any).id !== id);
        this.applyFilter();
        this.zone.run(() => this.cdr.detectChanges());
      },
      error: (err) => {
        alert((typeof err?.error === 'string' ? err.error : '') || 'Failed to delete book');
      }
    });
  }
}