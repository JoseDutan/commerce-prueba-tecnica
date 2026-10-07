import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ApiProblem, UploadResult } from '../../core/models/commerce.models';
import { SPANISH_PAGINATOR_PROVIDER } from '../../core/i18n/spanish-paginator-intl';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { commerceFileValidator } from '../../shared/utils/commerce-file.validators';
import { CsvPreview, CsvPreviewError, parseCsvPreview } from '../../shared/utils/csv-preview.parser';

/** Columna extra con el número de fila dentro del archivo. */
const ROW_NUMBER_COLUMN = '#';

/**
 * Pantalla de carga: el usuario elige commerce_DDMMYYYY.csv, revisa su
 * contenido en una tabla paginada y, si es correcto, lo envía a la API.
 */
@Component({
  selector: 'app-upload',
  standalone: true,
  providers: [SPANISH_PAGINATOR_PROVIDER],
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatTableModule,
  ],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UploadComponent {
  private readonly api = inject(CommerceApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly form = new FormGroup({
    file: new FormControl<File | null>(null, [Validators.required, commerceFileValidator]),
  });

  protected readonly pageSizeOptions = [10, 25, 50];

  /** Contenido del archivo elegido; null si no hay archivo válido. */
  protected readonly preview = signal<CsvPreview | null>(null);
  /** Error al leer el archivo en el navegador. */
  protected readonly parseError = signal<string | null>(null);
  /** Errores por línea devueltos por la API. */
  protected readonly serverErrors = signal<string[]>([]);
  protected readonly uploading = signal(false);
  protected readonly result = signal<UploadResult | null>(null);

  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(this.pageSizeOptions[0]);

  /** Identifica la última selección para descartar lecturas de archivos anteriores. */
  private selectionId = 0;

  protected readonly displayedColumns = computed(() => [ROW_NUMBER_COLUMN, ...(this.preview()?.headers ?? [])]);

  /** Solo se renderizan las filas de la página actual. */
  protected readonly pageRows = computed(() => {
    const start = this.pageIndex() * this.pageSize();
    return (this.preview()?.rows ?? []).slice(start, start + this.pageSize());
  });

  /** Número de la primera fila de la página, para la columna "#". */
  protected readonly pageOffset = computed(() => this.pageIndex() * this.pageSize());

  protected get fileControl(): FormControl<File | null> {
    return this.form.controls.file;
  }

  protected async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.item(0) ?? null;
    // Permite volver a elegir el mismo archivo después de corregirlo
    input.value = '';

    this.resetState();
    this.fileControl.setValue(file);
    this.fileControl.markAsTouched();

    if (!file || this.fileControl.invalid) {
      return;
    }

    const selection = ++this.selectionId;
    try {
      const preview = await parseCsvPreview(file);
      if (selection === this.selectionId) {
        this.preview.set(preview);
      }
    } catch (error) {
      if (selection !== this.selectionId) {
        return;
      }
      this.parseError.set(error instanceof CsvPreviewError ? error.message : 'No se pudo leer el archivo.');
    }
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }

  protected submit(): void {
    const file = this.fileControl.value;
    if (!file || this.form.invalid || !this.preview() || this.uploading()) {
      return;
    }

    this.uploading.set(true);
    this.serverErrors.set([]);

    this.api
      .upload(file)
      .pipe(
        finalize(() => this.uploading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (result) => {
          this.clear();
          this.result.set(result);
        },
        // El mensaje principal lo muestra el interceptor; aquí solo el detalle por línea
        error: (error: HttpErrorResponse) => this.serverErrors.set((error.error as ApiProblem | null)?.errors ?? []),
      });
  }

  protected clear(): void {
    this.form.reset();
    this.resetState();
  }

  private resetState(): void {
    this.selectionId++;
    this.preview.set(null);
    this.parseError.set(null);
    this.serverErrors.set([]);
    this.result.set(null);
    this.pageIndex.set(0);
  }
}
