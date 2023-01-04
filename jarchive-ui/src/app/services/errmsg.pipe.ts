import { HttpErrorResponse } from '@angular/common/http';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'errmsg'
})
export class ErrmsgPipe implements PipeTransform {

  transform(value: HttpErrorResponse, ...args: unknown[]): string {
    let message = value?.error?.message || value?.statusText || value?.message || 'Undescribed Error';
    // todo: if no spaces, split camel case
    if (message.indexOf(' ') < 0) {
      message = message.replace(/([a-z0-9])([A-Z])/g, '$1 $2');
    }
    return message;
  }

}
