import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

import { ApiProblem } from '../models/commerce.models';

/** Traduce un error HTTP a un mensaje comprensible para el usuario. */
export function toErrorMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'No se pudo conectar con el servidor. Verifique que la API esté en ejecución.';
  }

  const problem = error.error as ApiProblem | null;
  return problem?.detail ?? problem?.title ?? 'Ocurrió un error inesperado. Intente nuevamente.';
}

/**
 * Interceptor global de errores: muestra un aviso para cualquier respuesta
 * fallida y vuelve a lanzar el error para que la pantalla actualice su estado.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const snackBar = inject(MatSnackBar);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      snackBar.open(toErrorMessage(error), 'Cerrar', {
        duration: 6000,
        panelClass: 'snackbar-error',
      });
      return throwError(() => error);
    }),
  );
};
