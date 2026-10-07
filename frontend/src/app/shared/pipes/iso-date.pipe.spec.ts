import { IsoDatePipe } from './iso-date.pipe';

describe('IsoDatePipe', () => {
  const pipe = new IsoDatePipe();

  it('convierte yyyy-MM-dd a dd/MM/yyyy', () => {
    expect(pipe.transform('2026-10-06')).toBe('06/10/2026');
  });

  it('devuelve el valor original si no es una fecha ISO', () => {
    expect(pipe.transform('sin fecha')).toBe('sin fecha');
  });

  it('devuelve vacío para null o undefined', () => {
    expect(pipe.transform(null)).toBe('');
    expect(pipe.transform(undefined)).toBe('');
  });
});
