import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { ProcessResult, QuarantineRecord, UploadResult } from '../models/commerce.models';
import { CommerceApiService } from './commerce-api.service';

describe('CommerceApiService', () => {
  const baseUrl = `${environment.apiUrl}/commerce`;
  let service: CommerceApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CommerceApiService);
    http = TestBed.inject(HttpTestingController);
  });

  // Falla si quedó alguna petición sin esperar o repetida
  afterEach(() => http.verify());

  it('upload envía el archivo como multipart en el campo "file"', () => {
    const file = new File(['a,b'], 'commerce_06102026.csv', { type: 'text/csv' });
    const expected: UploadResult = { fileName: file.name, insertedCount: 19 };
    let result: UploadResult | undefined;

    service.upload(file).subscribe((r) => (result = r));

    const request = http.expectOne(`${baseUrl}/upload`);
    expect(request.request.method).toBe('POST');
    expect((request.request.body as FormData).get('file')).toEqual(file);
    request.flush(expected);
    expect(result).toEqual(expected);
  });

  it('process envía la fecha de proceso y devuelve el conteo en cuarentena', () => {
    const expected: ProcessResult = { processDate: '2026-10-06', quarantinedCount: 9 };
    let result: ProcessResult | undefined;

    service.process('2026-10-06').subscribe((r) => (result = r));

    const request = http.expectOne(`${baseUrl}/process`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ processDate: '2026-10-06' });
    request.flush(expected);
    expect(result).toEqual(expected);
  });

  it('getQuarantine hace un único GET', () => {
    let result: QuarantineRecord[] | undefined;

    service.getQuarantine().subscribe((r) => (result = r));

    const request = http.expectOne(`${baseUrl}/quarantine`);
    expect(request.request.method).toBe('GET');
    request.flush([]);
    expect(result).toEqual([]);
  });
});
