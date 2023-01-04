import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { timer } from 'rxjs';
import { first, tap } from 'rxjs/operators';
import { ClipboardService } from '../services/clipboard.service';
import { faClipboard, faClipboardCheck } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-clipspan',
  templateUrl: './clipspan.component.html',
  styleUrls: ['./clipspan.component.scss']
})
export class ClipspanComponent implements OnInit {
  @ViewChild('span') span!: ElementRef;
  clipped = false;
  hovering = false;
  icon = faClipboard;
  iconChecked = faClipboardCheck;

  constructor(
    private clipSvc: ClipboardService
  ) { }

  ngOnInit(): void {
  }

  hover(h: boolean): void {
    this.hovering = h;
  }

  copy(): void {
    this.clipSvc.copyToClipboard(this.span.nativeElement.innerText);
    this.clipped = true;
    timer(4000).pipe(
      tap(() => this.clipped = false),
      first()
    ).subscribe();
  }
}
