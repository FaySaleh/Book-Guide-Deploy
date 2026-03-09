import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

type BookStatus = 'to_read' | 'reading' | 'finished';

@Component({
  selector: 'app-book-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './book-details.html',
  styleUrls: ['./book-details.css']
})
export class BookDetailsComponent {
  id = 0;

  title = 'Atomic Habits';
  author = 'James Clear';
  description = 'A practical guide to building good habits and breaking bad ones.';
  status: BookStatus = 'to_read';
  rating = 0;

  constructor(route: ActivatedRoute) {
    this.id = Number(route.snapshot.paramMap.get('id') ?? 0);
  }

  setStatus(s: BookStatus) { this.status = s; }
  setRating(n: number) { this.rating = n; }

  statusLabel(s: BookStatus) {
    return s === 'to_read' ? 'To Read' : s === 'reading' ? 'Reading' : 'Finished';
  }
}
