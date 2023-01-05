import { HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faEarthAmericas, faEdit, faGear, faShareAlt, faShieldHalved, faUsers } from '@fortawesome/free-solid-svg-icons';
import { catchError, debounceTime, from, map, merge, mergeMap, Observable, of, Subject, switchMap, tap } from 'rxjs';
import { FileUploadStatus, Folder, FolderResponse } from '../api/api.models';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-folder',
  templateUrl: './folder.component.html',
  styleUrls: ['./folder.component.scss']
})
export class FolderComponent {
  response$: Observable<FolderResponse>;
  refresh$ = new Subject<string>();
  dropping$ = new Subject<File[]>();
  dropped$: Observable<any>;
  progress: FileUploadStatus[] = [];
  key = '';
  showInvite = false;
  showEditor = false;
  firstLook = false;
  faEdit = faEdit;
  faShare = faShareAlt;
  faPublic = faEarthAmericas;
  faInternal = faShieldHalved;
  faSpecified = faUsers;
  faGear = faGear;

  constructor(
    private router: Router,
    route: ActivatedRoute,
    api: ApiService
  ){
    this.response$ = merge(
      this.refresh$.pipe(debounceTime(1000)),
      route.params.pipe(map(p => p['key']))
    ).pipe(
      switchMap(key => api.retrieve(key)),
      tap(r => this.initView(r.folder))
    );

    this.dropped$ = this.dropping$.pipe(
      tap(l => this.uploadAdd(l)),
      switchMap(l => from(l)),
      mergeMap(f =>
        api.upload_file(this.key, f).pipe(
          tap(r => this.uploadProgress(r, f.name)),
          catchError(e => {
            this.uploadError(e, f.name);
            return of(e);
          })
        ),
        3 //concurrent
      )
    );
  }

  dropped(files: File[]): void {
    this.dropping$.next(files);
  }

  uploadAdd(files: File[]): void {
    files.forEach(f => {
      const existing = this.progress.find(i => i.name === f.name);
      if (!existing) {
        this.progress.push({name: f.name, progress: 0, start: Date.now()});
      }
    });
  }

  uploadProgress(e: HttpEvent<any>, name: string): void {
    const existing = this.progress.find(i => i.name === name);
    if (!!existing && e.type === HttpEventType.UploadProgress) {
      existing.progress = e.total ? Math.round(100 * e.loaded / e.total!) : 0;
      const percent = existing.progress / 100;
      const estimate = (Date.now() - existing.start) / percent;
      existing.eta = estimate - (estimate * percent);
      if (existing.progress === 100) {
        existing.eta = 0;
        this.refresh$.next(this.key);
      }
    }
    if (!existing && e.type === HttpEventType.Sent) {
      this.progress.push({name, progress: 0, start: Date.now()});
    }
  }

  uploadError(e: HttpErrorResponse, name: string): void {
    const existing = this.progress.find(i => i.name === name);
    if (!!existing) {
      existing.error = e;
      existing.progress = 0;
    }
  }

  updated(key: string): void {
    this.showEditor = false;
  }

  deleted(key: string): void {
    this.router.navigate(['/']);
  }

  initView(f: Folder): void {
    this.key = f.key;

    if (!this.firstLook) {
      this.firstLook = true;
      this.showEditor = f.name === 'New Folder';
    }

    this.progress = this.progress.filter(i => i.progress !== 100);
  }
}
