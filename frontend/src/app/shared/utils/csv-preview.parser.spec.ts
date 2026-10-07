import { COMMERCE_COLUMNS, CsvPreviewError, parseCsvPreview } from './csv-preview.parser';

describe('parseCsvPreview', () => {
  const header = COMMERCE_COLUMNS.join(',');
  const csvFile = (content: string) => new File([content], 'commerce_06102026.csv', { type: 'text/csv' });

  it('devuelve cabeceras y filas conservando acentos, ceros y vacíos', async () => {
    const file = csvFile(
      `${header}\n` +
        '2026-10-06,CED,0912345678,Panadería El Trigal,Juan Pérez,Cdla. Alborada,0991112233,a@mail.com,ACTIVO\n' +
        '2026-10-06,RUC,,   ,Sin Datos S.A.,Calle 1,042000000,b@mail.com,ACTIVO\n',
    );

    const preview = await parseCsvPreview(file);

    expect(preview.headers).toEqual([...COMMERCE_COLUMNS]);
    expect(preview.rows.length).toBe(2);
    expect(preview.rows[0][2]).toBe('0912345678');
    expect(preview.rows[0][3]).toBe('Panadería El Trigal');
    expect(preview.rows[1][2]).toBe('');
  });

  it('rechaza un archivo sin contenido', async () => {
    await expectAsync(parseCsvPreview(csvFile(''))).toBeRejectedWithError(CsvPreviewError, /vacío/);
  });

  it('rechaza un archivo con solo la cabecera', async () => {
    await expectAsync(parseCsvPreview(csvFile(`${header}\n`))).toBeRejectedWithError(CsvPreviewError, /vacío/);
  });

  it('rechaza una cabecera distinta a la esperada', async () => {
    await expectAsync(parseCsvPreview(csvFile('fecha,documento\n2026-10-06,123\n'))).toBeRejectedWithError(
      CsvPreviewError,
      /cabecera/,
    );
  });
});
