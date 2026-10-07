import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const FILE_NAME_PATTERN = /^commerce_(\d{2})(\d{2})(\d{4})\.csv$/i;

/**
 * Indica si el nombre cumple commerce_DDMMYYYY.csv y la fecha existe en el
 * calendario. Es la misma regla que aplica la API (FileNameValidator).
 */
export function isValidCommerceFileName(fileName: string): boolean {
  const match = FILE_NAME_PATTERN.exec(fileName);
  if (!match) {
    return false;
  }

  const [day, month, year] = match.slice(1).map(Number);
  // Date ajusta las fechas imposibles (31/02 pasa a marzo): si cambia, no era válida
  const date = new Date(year, month - 1, day);
  return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
}

/**
 * Validador de formulario reactivo para el archivo de carga.
 * Errores posibles: `fileName` (nombre inválido) y `emptyFile` (0 bytes).
 */
export const commerceFileValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const file = control.value as File | null;
  if (!file) {
    return null; // la obligatoriedad la cubre Validators.required
  }

  if (!isValidCommerceFileName(file.name)) {
    return { fileName: true };
  }

  return file.size === 0 ? { emptyFile: true } : null;
};
