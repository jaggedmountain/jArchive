import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { map, Observable } from 'rxjs';
import { ApiService } from '../api/api.service';
import { ConfigService } from '../services/config.service';

@Component({
  selector: 'app-reader',
  templateUrl: './reader.component.html',
  styleUrls: ['./reader.component.scss']
})
export class ReaderComponent {
  url$: Observable<string>;

  constructor(
    route: ActivatedRoute,
    api: ApiService,
    conf: ConfigService
  ) {

    this.url$ = api.reader().pipe(
      map(() => conf.apphost + route.snapshot.queryParams['ReturnUrl']),
    );
  }

}
