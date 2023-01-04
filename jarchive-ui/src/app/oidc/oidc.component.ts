import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-oidc',
  templateUrl: './oidc.component.html',
  styleUrls: ['./oidc.component.scss']
})
export class OidcComponent {
  message = '';

  constructor(
    auth: AuthService,
    router: Router
  ) {
    auth.externalLoginCallback().then(
      (user) => {
        router.navigateByUrl(user.state || '/');
      },
      (err) => {
        console.log(err);
        this.message = (err.error || err).message;
      }
    );
  }
}
