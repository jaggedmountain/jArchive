import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './services/auth.guard';
import { FileComponent } from './file/file.component';
import { FolderComponent } from './folder/folder.component';
import { FoldersComponent } from './folders/folders.component';
import { LoginComponent } from './login/login.component';
import { OidcComponent } from './oidc/oidc.component';
import { ReaderComponent } from './reader/reader.component';
import { RedeemComponent } from './redeem/redeem.component';

const routes: Routes = [
  { path: '', component: FoldersComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'oidc', component: OidcComponent },
  { path: 'redeem', children: [
    { path: ':token', component: RedeemComponent },
    { path: '', component: RedeemComponent },
  ]},
  { path: 'file/:key/:folder/:name', component: FileComponent },
  { path: 'folder/:key/:name', component: FolderComponent },
  { path: 'reader', canActivateChild: [AuthGuard], children: [
    { path: '**', component: ReaderComponent },
  ]},

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
