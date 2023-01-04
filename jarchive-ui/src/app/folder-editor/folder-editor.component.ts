import { Component, EventEmitter, Input, Output } from '@angular/core';
import { faBan, faEarthAmericas, faShieldAlt, faShieldHalved, faTrashCan, faUnlock, faUpload, faUsers } from '@fortawesome/free-solid-svg-icons';
import { ChangedFolder, Folder } from '../api/api.models';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-folder-editor',
  templateUrl: './folder-editor.component.html',
  styleUrls: ['./folder-editor.component.scss']
})
export class FolderEditorComponent {
  @Input() folder!: Folder;
  @Output() deleted = new EventEmitter<string>();
  @Output() updated = new EventEmitter<string>();
  faTrash = faTrashCan;
  faBan = faBan;
  faUpdate = faUpload;
  faPublic = faEarthAmericas;
  faInternal = faShieldHalved;
  faSpecified = faUsers;

  constructor(
    private api: ApiService
  ){

  }

  update(): void {
    this.api.update(this.folder as ChangedFolder).subscribe(
      () => this.updated.next(this.folder.key)
    );
  }

  reset(): void {
    this.api.reset(this.folder.key).subscribe(
      () => this.updated.next(this.folder.key)
    );
  }

  delete(): void {
    this.api.delete(this.folder.key)
    .subscribe(() =>this.deleted.next(this.folder.key));
  }
}
