import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filesize'
})
export class FilesizePipe implements PipeTransform {

  transform(value: number, ...args: unknown[]): string {
    const suffix = [' KB', ' MB', ' GB', ' TB'];
    let divisor = 1000;
    let index = 0;
    let val = value;
    while (index < suffix.length) {
      val = value / divisor;
      if (val < 1000) {
        return Math.round(val) + suffix[index];
      }
      index += 1;
      divisor *= 1000;
    }
    return Math.round(value) + ' B';
  }

}
