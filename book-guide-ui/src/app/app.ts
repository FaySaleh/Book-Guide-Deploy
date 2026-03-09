import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { NavbarComponent } from './components/navbar/navbar';
import { LangService } from './services/lang.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavbarComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})

export class App {
  hideNavbar = false;


  constructor(private router: Router,private lang: LangService) {
        this.lang.init();

    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => {
        const url = this.router.url;
this.hideNavbar =
  url.startsWith('/login') ||
  url.startsWith('/register') ||
  url.startsWith('/forgot-password') ||
  url.startsWith('/reset-password');
      });
  }
}
