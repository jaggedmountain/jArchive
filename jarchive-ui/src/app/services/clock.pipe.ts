import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'clock'
})
export class ClockPipe implements PipeTransform {

  transform(value?: number, ...args: unknown[]): string {
    if (!value) { return ''; }
    value = value / 1000;
    const h = Math.floor(value / 3600);
    const m = Math.floor((value - (h * 3600)) / 60);
    const s = Math.floor(value - (h * 3600) - (m * 60));

    return `${h>0 ? ('' + h).padStart(2, '0') + ':' : ''}${('' + m).padStart(2, '0')}:${('' + s).padStart(2, '0')}`;
  }

}
