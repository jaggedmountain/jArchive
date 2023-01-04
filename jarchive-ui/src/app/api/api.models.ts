import { HttpErrorResponse } from "@angular/common/http";

export interface Folder {
  key: string;
  name: string;
  description: string;
  scope: FolderScope;
  role: FolderRole;
  files: File[];
  isOwned: boolean;
  canWrite: boolean;
}

export interface ChangedFolder {
  key: string;
  name: string;
  description: string;
  scope: FolderScope;
}

export interface NewFolder {
  name: string;
}

export interface File {
  name: string;
  length: number;
  creationTime: string;
  url: string;
}

export interface ChangedFile {
  key: string;
  oldName: string;
  newName: string;
}

export enum FolderScope {
  specified = 'specified',
  internal = 'internal',
  public = 'public'
}

export enum FolderRole {
  none = 'none',
  reader = 'reader',
  writer = 'writer',
  owner = 'owner',
  admin = 'admin'
}

export interface Invitation {
  key: string;
  role: FolderRole;
  expirationMinutes: number;
  multipleUse: boolean;
  token: string;
}

export const MaxNameLength = 255;
export const MaxDescriptionLength = 1024;

export interface FolderResponse {
  folder: Folder;
  error: HttpErrorResponse;
}
