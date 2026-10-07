import { Injectable, Provider } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';

/** Textos en español para todos los paginadores de la aplicación. */
@Injectable()
export class SpanishPaginatorIntl extends MatPaginatorIntl {
  override itemsPerPageLabel = 'Registros por página:';
  override nextPageLabel = 'Página siguiente';
  override previousPageLabel = 'Página anterior';
  override firstPageLabel = 'Primera página';
  override lastPageLabel = 'Última página';

  override getRangeLabel = (page: number, pageSize: number, length: number): string => {
    if (length === 0 || pageSize === 0) {
      return `0 de ${length}`;
    }

    const start = page * pageSize + 1;
    const end = Math.min(start + pageSize - 1, length);
    return `${start} – ${end} de ${length}`;
  };
}

/**
 * Proveedor para los componentes que usan un paginador. Se declara en cada
 * pantalla (carga diferida) y no en el arranque, para no llevar el paginador
 * al paquete inicial.
 */
export const SPANISH_PAGINATOR_PROVIDER: Provider = { provide: MatPaginatorIntl, useClass: SpanishPaginatorIntl };
