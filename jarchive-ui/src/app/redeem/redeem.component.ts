import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { catchError, filter, map, merge, Observable, of, Subject, switchMap } from 'rxjs';
import { FolderResponse } from '../api/api.models';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-redeem',
  templateUrl: './redeem.component.html',
  styleUrls: ['./redeem.component.scss']
})
export class RedeemComponent {
  token = '';
  token$ = new Subject<string>();
  folder$: Observable<FolderResponse>;

  constructor(
    api: ApiService,
    route: ActivatedRoute
  ) {
    this.folder$ = merge(
      this.token$,
      route.params.pipe(map(p => p['token']))
    ).pipe(
      filter(t => !!t),
      switchMap(t => api.redeem(t)),
      catchError(e => of(e))
    );

  }

  redeem(): void {
    this.token$.next(this.token);
  }
}
