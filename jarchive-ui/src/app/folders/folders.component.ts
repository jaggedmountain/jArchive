import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { faAdd, faEarthAmericas, faFolderPlus, faShieldHalved, faUsers, faUserShield } from '@fortawesome/free-solid-svg-icons';
import { BehaviorSubject, first, Observable } from 'rxjs';
import { Folder, NewFolder } from '../api/api.models';
import { ApiService } from '../api/api.service';
import { AuthUserState } from '../services/auth.service';
import { ConfigService } from '../services/config.service';

@Component({
  selector: 'app-folders',
  templateUrl: './folders.component.html',
  styleUrls: ['./folders.component.scss']
})
export class FoldersComponent {
  folders$: Observable<Folder[]>;
  userState$: BehaviorSubject<AuthUserState>;
  faAdd = faFolderPlus;
  faRedeem = faUserShield;
  faPublic = faEarthAmericas;
  faInternal = faShieldHalved;
  faSpecified = faUsers;

  constructor(
    private api: ApiService,
    private router: Router,
    config: ConfigService
  ){
    this.folders$ = api.list('');
    this.userState$ = config.userState$;
  }

  create(): void {
    this.api.create({} as NewFolder).pipe(
      first()
    ).subscribe(folder =>
      this.router.navigate(['/folder', folder.key, folder.name]
    ));
  }
}
