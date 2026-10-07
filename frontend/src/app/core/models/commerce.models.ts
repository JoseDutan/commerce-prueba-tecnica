/** Contratos de la API de comercios. Reflejan los DTO del backend .NET. */

/** Respuesta de POST /commerce/upload. */
export interface UploadResult {
  fileName: string;
  insertedCount: number;
}

/** Respuesta de POST /commerce/process. */
export interface ProcessResult {
  /** Fecha de proceso en formato yyyy-MM-dd. */
  processDate: string;
  quarantinedCount: number;
}

/** Comercio en cuarentena (GET /commerce/quarantine). */
export interface QuarantineRecord {
  id: number;
  /** Fecha de proceso en formato yyyy-MM-dd. */
  pcProcessDate: string;
  pcTipDoc: string | null;
  pcNumDoc: string | null;
  pcNomComRed: string | null;
  pcNomComLegal: string | null;
  pcDireccion: string | null;
  pcTelefono: string | null;
  pcEmail: string | null;
  pcEstado: string | null;
  /** Motivos del rechazo, unidos por " | " si son varios. */
  motivo: string;
}

/** Cuerpo de error de la API (ProblemDetails, RFC 7807). */
export interface ApiProblem {
  title?: string;
  status?: number;
  detail?: string;
  /** Detalle opcional, por ejemplo los errores por línea del CSV. */
  errors?: string[];
}
