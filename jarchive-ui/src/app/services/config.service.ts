import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { UserManagerSettings } from 'oidc-client';
import { catchError, tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Location, PlatformLocation } from '@angular/common';
import { AuthUserState } from './auth.service';

@Injectable({providedIn: 'root'})
export class ConfigService {

  storageKey = 'jarchive';
  settings: Settings = environment.settings;
  local: LocalAppSettings = {};
  tabs: TabRef[] = [];
  settings$ = new BehaviorSubject<Settings>(this.settings);
  userState$ = new BehaviorSubject<AuthUserState>({} as AuthUserState);
  basehref: string;
  absoluteUrl: string;

  constructor(
    private http: HttpClient,
    private location: Location,
    platform: PlatformLocation
  ) {
    this.basehref = platform.getBaseHrefFromDOM();
    this.absoluteUrl = `${window.location.protocol}//${window.location.host}${this.basehref}`;
    this.local = this.getLocal();
  }

  get currentPath(): string {
    return this.location.path();
  }

  // use setting, or relative
  get apphost(): string {
    const v = this.settings.apphost || this.basehref;
    return this.location.normalize(v);
  }

  load(): Observable<any> {
    return this.http.get<Settings>('assets/settings.json')
      .pipe(
        catchError((err: Error) => {
          return of({} as Settings);
        }),
        tap(s => {
          this.settings = {...this.settings, ...s};
          this.settings.oidc = {...this.settings.oidc, ...s.oidc};
          this.settings$.next(this.settings);
        })
      );
  }

  showTab(url: string): void {
    let item = this.tabs.find(t => t.url === url);

    if (!item) {
      item = {url, window: null};
      this.tabs.push(item);
    }

    if (!item.window || item.window.closed) {
        item.window = window.open(url);
    } else {
        item.window.focus();
    }
  }

  updateLocal(model: LocalAppSettings): void {
    this.local = {...this.local, ...model};
    this.storeLocal(this.local);
  }

  storeLocal(model: LocalAppSettings): void {
    try {
      window.localStorage[this.storageKey] = JSON.stringify(model);
    } catch (e) {
    }
  }
  getLocal(): LocalAppSettings {
    try {
        return JSON.parse(window.localStorage[this.storageKey] || {});
    } catch (e) {
        return {};
    }
  }
  clearStorage(): void {
    try {
        window.localStorage.removeItem(this.storageKey);
    } catch (e) { }
  }

  setAuthState(s: AuthUserState) {
    this.userState$.next(s);
  }
}

export interface LocalAppSettings {
  lastKey?: string;
  inviteExpirationMinutes?: number;
  inviteMulituse?: boolean;
}

export interface Settings {
  appname: string;
  apphost: string;
  oidc: AppUserManagerSettings;
}

export interface AppUserManagerSettings extends UserManagerSettings {
  useLocalStorage?: boolean;
  silentRenewIfActiveSeconds?: number;
  debug?: boolean;
}

export interface TabRef {
  url: string;
  window: Window | null;
}
