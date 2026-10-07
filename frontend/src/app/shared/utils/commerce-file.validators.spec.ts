import { FormControl } from '@angular/forms';

import { commerceFileValidator, isValidCommerceFileName } from './commerce-file.validators';

describe('isValidCommerceFileName', () => {
  const valid = ['commerce_06102026.csv', 'commerce_29022024.csv', 'COMMERCE_01012025.CSV'];
  const invalid = [
    '',
    'datos.csv',
    'commerce_06102026.txt',
    'commerce_0610206.csv', // 7 dígitos
    'commerce_2026-10-06.csv', // otro formato de fecha
    'commerce_31022026.csv', // 31 de febrero no existe
    'commerce_29022025.csv', // 2025 no es bisiesto
    'commerce_06132026.csv', // mes 13
    'x_commerce_06102026.csv',
    'commerce_06102026.csv.exe',
  ];

  valid.forEach((name) => it(`acepta "${name}"`, () => expect(isValidCommerceFileName(name)).toBeTrue()));

  invalid.forEach((name) => it(`rechaza "${name}"`, () => expect(isValidCommerceFileName(name)).toBeFalse()));
});

describe('commerceFileValidator', () => {
  const validate = (file: File | null) => commerceFileValidator(new FormControl(file));

  it('no reporta error sin archivo (lo cubre required)', () => {
    expect(validate(null)).toBeNull();
  });

  it('reporta fileName si el nombre es inválido', () => {
    expect(validate(new File(['a'], 'datos.csv'))).toEqual({ fileName: true });
  });

  it('reporta emptyFile si el archivo tiene 0 bytes', () => {
    expect(validate(new File([], 'commerce_06102026.csv'))).toEqual({ emptyFile: true });
  });

  it('acepta un archivo con nombre válido y contenido', () => {
    expect(validate(new File(['a'], 'commerce_06102026.csv'))).toBeNull();
  });
});
