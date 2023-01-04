import { Component, EventEmitter, Input, Output } from '@angular/core';
import { faCheck, faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-confirm-button',
  templateUrl: './confirm-button.component.html',
  styleUrls: ['./confirm-button.component.scss']
})
export class ConfirmButtonComponent {
  @Input() btnClass = 'btn btn-info btn-sm';
  @Input() cancelButtonClass = 'btn btn-outline-secondary';
  @Input() disabled = false;
  @Output() confirm = new EventEmitter<boolean>();
  confirming = false;

  faCheck = faCheck;
  faTimes = faTimes;

  constructor() { }

  ngOnInit(): void {
  }

  continue(yes?: boolean): void {
    if (!!yes) { this.confirm.emit(true); }
    this.confirming = false;
  }

}
