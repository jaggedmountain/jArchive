import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faFilePen, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { map, Observable, first, Subject, merge } from 'rxjs';
import { ChangedFile } from '../api/api.models';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.scss']
})
export class FileComponent {
    response$: Observable<FileParams>;
    refresh$ = new Subject<FileParams>();
    newName = '';
    faRename = faFilePen;
    faTrash = faTrashAlt;

    constructor(
      private api: ApiService,
      private router: Router,
      route: ActivatedRoute,
    ){
      this.response$ = merge(
        this.refresh$,
        route.params.pipe(
          map(p => p as FileParams)
        )
      );
    }

    rename(p: FileParams): void {
      if (!this.newName) { return; }

      const model: ChangedFile = {
        key: p.key,
        oldName: p.name,
        newName: this.newName
      };

      this.api.rename_file(model).pipe(
        first()
      ).subscribe(() => {
        this.newName = '';
        this.refresh$.next({
          key: p.key,
          folder: p.folder,
          name: model.newName
        });
      });
    }

    delete(p: FileParams): void {
      this.api.delete_file(p.key, p.name).subscribe(
        () => this.router.navigate(['/folder', p.key, p.folder])
      );
    }
}

export interface FileParams {
  key: string;
  folder: string;
  name: string;
}
