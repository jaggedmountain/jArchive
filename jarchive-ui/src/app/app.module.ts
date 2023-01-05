import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FoldersComponent } from './folders/folders.component';
import { FolderComponent } from './folder/folder.component';
import { FileComponent } from './file/file.component';
import { ConfigService } from './services/config.service';
import { Observable } from 'rxjs';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './services/auth.interceptor';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { LoginComponent } from './login/login.component';
import { OidcComponent } from './oidc/oidc.component';
import { ErrmsgPipe } from './services/errmsg.pipe';
import { DropzoneComponent } from './dropzone/dropzone.component';
import { ReaderComponent } from './reader/reader.component';
import { InviteComponent } from './invite/invite.component';
import { ConfirmButtonComponent } from './confirm-button/confirm-button.component';
import { SpinnerComponent } from './spinner/spinner.component';
import { FormsModule } from '@angular/forms';
import { FilesizePipe } from './services/filesize.pipe';
import { ButtonsModule } from 'ngx-bootstrap/buttons'
import { ClipspanComponent } from './clipspan/clipspan.component';
import { RedeemComponent } from './redeem/redeem.component';
import { FolderEditorComponent } from './folder-editor/folder-editor.component';
import { ProgressbarModule } from 'ngx-bootstrap/progressbar';
import { ClockPipe } from './services/clock.pipe';

@NgModule({
  declarations: [
    AppComponent,
    FoldersComponent,
    FolderComponent,
    FileComponent,
    LoginComponent,
    OidcComponent,
    DropzoneComponent,
    ErrmsgPipe,
    ReaderComponent,
    InviteComponent,
    ConfirmButtonComponent,
    SpinnerComponent,
    FilesizePipe,
    ClipspanComponent,
    RedeemComponent,
    FolderEditorComponent,
    ClockPipe
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    FontAwesomeModule,
    ButtonsModule.forRoot(),
    ProgressbarModule.forRoot()
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: loadSettings,
      deps: [ConfigService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }


export function loadSettings(
  config: ConfigService,
): (() => Observable<any>) {
  return (): Observable<any> => config.load();
}
