import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ProcessResult, QuarantineRecord, UploadResult } from '../models/commerce.models';

/**
 * Acceso HTTP a la API de comercios. Es el único punto de la aplicación que
 * conoce las URL del backend.
 */
@Injectable({ providedIn: 'root' })
export class CommerceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/commerce`;

  /** Envía el archivo commerce_DDMMYYYY.csv para insertarlo en commerce. */
  upload(file: File): Observable<UploadResult> {
    const body = new FormData();
    body.append('file', file, file.name);
    return this.http.post<UploadResult>(`${this.baseUrl}/upload`, body);
  }

  /** Procesa los comercios de una fecha (yyyy-MM-dd). */
  process(processDate: string): Observable<ProcessResult> {
    return this.http.post<ProcessResult>(`${this.baseUrl}/process`, { processDate });
  }

  /** Lista los comercios en cuarentena con su motivo. */
  getQuarantine(): Observable<QuarantineRecord[]> {
    return this.http.get<QuarantineRecord[]>(`${this.baseUrl}/quarantine`);
  }
}
