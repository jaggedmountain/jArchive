// Copyright 2021 Carnegie Mellon University. All Rights Reserved.
// Released under a MIT (SEI)-style license. See LICENSE.md in the project root for license information.

import { Component, EventEmitter, HostListener, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-dropzone',
  templateUrl: './dropzone.component.html',
  styleUrls: ['./dropzone.component.scss']
})
export class DropzoneComponent implements OnInit {
  @Input() inputId = 'dropzone-input';
  @Input() btnClass = 'btn btn-sm btn-secondary';
  @Output() dropped = new EventEmitter<File[]>();
  dropzone = false;

  constructor() { }

  ngOnInit(): void {
  }

  //
  // Handle component events
  //
  filesSelected(ev: any): void {
    this.dropped.emit(Array.from(ev.target.files));
  }

  //
  // Handle drag/drop events
  //
  @HostListener('dragenter', ['$event'])
  @HostListener('dragover', ['$event'])
  onEnter(event: DragEvent): boolean {
    this.dropzone = true;
    return false;
  }
  @HostListener('dragleave', ['$event'])
  @HostListener('dragexit', ['$event'])
  onLeave(event: DragEvent): boolean {
    this.dropzone = false;
    return false;
  }
  @HostListener('drop', ['$event'])
  onDrop(event: DragEvent): boolean {
    this.dropzone = false;
    this.dropped.emit(Array.from(event.dataTransfer?.files || []));
    return false;
  }
}
