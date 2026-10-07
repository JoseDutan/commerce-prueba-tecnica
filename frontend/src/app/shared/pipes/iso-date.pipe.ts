import { Pipe, PipeTransform } from '@angular/core';

/**
 * Muestra una fecha ISO (yyyy-MM-dd) como dd/MM/yyyy.
 * Trabaja con el texto, sin crear un Date, para que la zona horaria del
 * navegador no pueda desplazar el día.
 */
@Pipe({ name: 'isoDate', standalone: true })
export class IsoDatePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value ?? '');
    return match ? `${match[3]}/${match[2]}/${match[1]}` : (value ?? '');
  }
}
