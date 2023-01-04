import { HttpClient, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { ConfigService } from '../services/config.service';
import { ChangedFile, ChangedFolder, Folder, FolderResponse, FolderRole, Invitation, NewFolder } from './api.models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  url = '';

  constructor(
    private http: HttpClient,
    config: ConfigService
  ) {
    this.url = config.apphost + '/api';
  }

  public list(filter: string): Observable<Folder[]> {
    return this.http.get<Folder[]>(`${this.url}/folders`, { params: {filter}}).pipe(
      map(list => list.sort((a, b) => a.name < b.name ? -1 : a.name > b.name ? 1 : 0))
    );
  }

  public create(model: NewFolder): Observable<Folder> {
    return this.http.post<Folder>(`${this.url}/folder`, model);
  }

  public retrieve(key: string): Observable<FolderResponse> {
    return this.http.get<Folder>(`${this.url}/folder/${key}`).pipe(
      map(folder =>
        ({folder: {
          ...folder,
          files: folder.files.sort((a, b) => a.name < b.name ? -1 : a.name > b.name ? 1 : 0),
          isOwned: folder.role===FolderRole.owner||folder.role===FolderRole.admin,
          canWrite: folder.role===FolderRole.writer||folder.role===FolderRole.owner||folder.role===FolderRole.admin,
        }} as FolderResponse)
      ),
      catchError(error => of({error} as FolderResponse))
    );
  }

  public update(model: ChangedFolder): Observable<any> {
    return this.http.put<any>(`${this.url}/folder`, model);
  }

  public delete(key: string): Observable<any> {
    return this.http.delete<any>(`${this.url}/folder/${key}`);
  }

  public invite(model: Invitation): Observable<Invitation> {
    return this.http.post<Invitation>(`${this.url}/folder/invite`, model);
  }

  public redeem(token: string): Observable<FolderResponse> {
    return this.http.put<Folder>(`${this.url}/folder/redeem/${token}`, {}).pipe(
      map(folder => ({folder} as FolderResponse)),
      catchError(error => of({error} as FolderResponse))
    );
  }

  public reset(key: string): Observable<Folder> {
    return this.http.post<Folder>(`${this.url}/folder/reset/${key}`, {});
  }

  public rename_file(model: ChangedFile): Observable<any> {
    return this.http.put<any>(`${this.url}/file/rename`, model);
  }

  public delete_file(key: string, name: string): Observable<any> {
    return this.http.delete<any>(`${this.url}/file/${key}/${name}`);
  }

  public upload_file(key: string, file: File): Observable<any> {
    const payload: FormData = new FormData();
    payload.append('file', file, file.name);
    return this.http.request<any>(
        new HttpRequest('POST', `${this.url}/file/upload/${key}`, payload, { reportProgress: true})
    );
  }

  public reader(): Observable<any> {
    return this.http.get<any>(`${this.url}/reader`);
  }

  public reader_signout(): Observable<any> {
    return this.http.post<any>(`${this.url}/reader/signout`, {});
  }
}
