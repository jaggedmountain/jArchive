import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { faOpenid } from '@fortawesome/free-brands-svg-icons';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  authority = "";
  working = false;
  returnUrl = "";
  faOpenid = faOpenid;

  constructor(
    private auth: AuthService,
    route: ActivatedRoute
  ) {
    this.authority = auth.authority;
    this.returnUrl = route.snapshot.queryParams['ReturnUrl'];
  }

  ngOnInit(): void {
  }

  login(): void {
    this.working = true;
    const url = !!this.returnUrl
      ? this.returnUrl
      : this.auth.redirectUrl
    ;

    this.auth.externalLogin(url);
  }
}
