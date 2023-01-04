import { Component, Input, OnInit } from '@angular/core';
import { faClipboard, faGear, faGlasses, faPen, faShieldAlt } from '@fortawesome/free-solid-svg-icons';
import { first } from 'rxjs';
import { Folder, FolderRole, Invitation } from '../api/api.models';
import { ApiService } from '../api/api.service';
import { ConfigService } from '../services/config.service';

@Component({
  selector: 'app-invite',
  templateUrl: './invite.component.html',
  styleUrls: ['./invite.component.scss']
})
export class InviteComponent implements OnInit {
  @Input() folder: Folder = {} as Folder;
  invitation: Invitation = {} as Invitation;
  redeemUrl = '';
  faGear = faGear;
  faClipboard = faClipboard;
  faReader = faGlasses;
  faWriter = faPen;
  faOwner = faShieldAlt;

  constructor(
    private api: ApiService,
    private conf: ConfigService
  ){
  }

  ngOnInit(): void {
    this.invitation.key = this.folder.key;
    this.invitation.role = FolderRole.reader;
    this.invitation.multipleUse = this.conf.local.inviteMulituse || false;
    this.invitation.expirationMinutes = this.conf.local.inviteExpirationMinutes || 5;
  }

  invite(): void {
    this.conf.updateLocal({
      inviteExpirationMinutes: this.invitation.expirationMinutes,
      inviteMulituse: this.invitation.multipleUse
    });

    this.api.invite(this.invitation).pipe(
      first()
    ).subscribe(result => {
      this.invitation = result;
      this.redeemUrl = `redeem/${result.token}`;
    });
  }

}
