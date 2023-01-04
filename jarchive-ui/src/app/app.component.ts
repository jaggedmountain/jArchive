import { DOCUMENT } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { faFolderOpen } from '@fortawesome/free-regular-svg-icons';
import { faUserShield } from '@fortawesome/free-solid-svg-icons';
import { ApiService } from './api/api.service';
import { AuthService, AuthTokenState } from './services/auth.service';
import { ConfigService } from './services/config.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'jarchive-ui';
  logged_in = false;
  expiring = false;
  faFolder = faFolderOpen;
  faUser = faUserShield;

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private config: ConfigService,
    private authSvc: AuthService,
    private api: ApiService,
    router: Router,
    titleSvc: Title
  ){
    this.title = config.settings.appname;

    titleSvc.setTitle(this.title);

    authSvc.tokenState$.subscribe(token => {
      this.logged_in =
        token === AuthTokenState.valid ||
        token === AuthTokenState.expiring
      ;

      this.expiring = token === AuthTokenState.expiring;

      if (token === AuthTokenState.expired) {
        router.navigate(['/']);
      }
    });
  }

  login(): void {
    this.authSvc.externalLogin(this.config.currentPath)
  }

  logout(): void {
    this.api.reader_signout().subscribe(() =>
      this.authSvc.logout()
    );
  }

  reader(): void {
    this.api.reader().subscribe();
  }

  ngOnInit() {
    this.document.documentElement.setAttribute(
      'data-bs-theme',
      window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
    );
  }
}
