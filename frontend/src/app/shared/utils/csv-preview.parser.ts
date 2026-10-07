import Papa from 'papaparse';

/** Columnas esperadas en commerce_DDMMYYYY.csv, en orden. */
export const COMMERCE_COLUMNS: readonly string[] = [
  'pc_processdate',
  'pc_tipdoc',
  'pc_numdoc',
  'pc_nomcomred',
  'pc_nomcomlegal',
  'pc_direccion',
  'pc_telefono',
  'pc_email',
  'pc_estado',
];

/** Contenido del CSV listo para previsualizar. */
export interface CsvPreview {
  headers: string[];
  /** Filas de datos; cada celda en la posición de su cabecera. */
  rows: string[][];
}

/** Error de lectura del CSV con un mensaje apto para mostrar al usuario. */
export class CsvPreviewError extends Error {}

/**
 * Lee el CSV en el navegador (UTF-8) para previsualizarlo antes de enviarlo.
 * Solo comprueba la estructura; las reglas de negocio las aplica el backend.
 *
 * @throws CsvPreviewError si el archivo está vacío o la cabecera no coincide.
 */
export function parseCsvPreview(file: File): Promise<CsvPreview> {
  return new Promise((resolve, reject) => {
    Papa.parse<string[]>(file, {
      encoding: 'UTF-8',
      skipEmptyLines: true,
      complete: ({ data }) => {
        try {
          resolve(toPreview(data));
        } catch (error) {
          reject(error);
        }
      },
      error: () => reject(new CsvPreviewError('No se pudo leer el archivo.')),
    });
  });
}

function toPreview(data: string[][]): CsvPreview {
  if (data.length === 0) {
    throw new CsvPreviewError('El archivo está vacío.');
  }

  const [headerRow, ...rows] = data;
  const headers = headerRow.map((header) => header.trim().toLowerCase());

  const headerMatches =
    headers.length === COMMERCE_COLUMNS.length &&
    COMMERCE_COLUMNS.every((column, index) => column === headers[index]);
  if (!headerMatches) {
    throw new CsvPreviewError(`La cabecera no es válida. Se esperaba: ${COMMERCE_COLUMNS.join(',')}`);
  }

  if (rows.length === 0) {
    throw new CsvPreviewError('El archivo está vacío: no contiene registros.');
  }

  return { headers, rows };
}
