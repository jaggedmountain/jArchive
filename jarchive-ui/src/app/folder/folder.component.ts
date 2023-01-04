import { HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faEarthAmericas, faEdit, faGear, faShareAlt, faShieldHalved, faUsers } from '@fortawesome/free-solid-svg-icons';
import { catchError, debounceTime, from, last, map, merge, mergeMap, Observable, of, Subject, switchMap, tap } from 'rxjs';
import { Folder, FolderResponse } from '../api/api.models';
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
  progress: any[] = [];
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
      tap(key => this.key = key),
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
          }),
          last()
        ),
        3 //concurrent
      )
    );
  }

  dropped(files: File[]): void {
    this.dropping$.next(files);
  }

  uploadAdd(files: File[]): void {
    this.progress = this.progress.filter(i => i.progress !== 100);
    files.forEach(f => {
      const existing = this.progress.find(i => i.name === f.name);
      if (!existing) {
        this.progress.push({name: f.name, progress: 0});
      }
    });
  }

  uploadProgress(e: HttpEvent<any>, f: string): void {
    const existing = this.progress.find(i => i.name === f);
    if (!!existing && e.type === HttpEventType.UploadProgress) {
      existing.progress = e.total ? Math.round(100 * e.loaded / e.total!) : 0;
      if (existing.progress === 100) {
        this.refresh$.next(this.key);
      }
    }
  }

  uploadError(e: HttpErrorResponse, f: string): void {
    const existing = this.progress.find(i => i.name === f);
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
    if (!this.firstLook) {
      this.firstLook = true;
      this.showEditor = f.name === 'New Folder';
    }
  }
}
