import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';

import { QuarantineRecord } from '../../core/models/commerce.models';
import { SPANISH_PAGINATOR_PROVIDER } from '../../core/i18n/spanish-paginator-intl';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { IsoDatePipe } from '../../shared/pipes/iso-date.pipe';

/** Separador con el que el backend une los motivos de un mismo registro. */
const REASON_SEPARATOR = ' | ';

/** Registro de cuarentena con los motivos ya separados para mostrarlos en lista. */
interface QuarantineRow extends QuarantineRecord {
  reasons: string[];
}

/**
 * Pantalla de cuarentena: lista los comercios rechazados al procesar y el
 * motivo de cada rechazo.
 */
@Component({
  selector: 'app-quarantine',
  standalone: true,
  providers: [SPANISH_PAGINATOR_PROVIDER],
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatTableModule,
    IsoDatePipe,
  ],
  templateUrl: './quarantine.component.html',
  styleUrl: './quarantine.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuarantineComponent {
  private readonly api = inject(CommerceApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly displayedColumns = [
    'pcProcessDate',
    'pcTipDoc',
    'pcNumDoc',
    'pcNomComRed',
    'motivo',
    'pcNomComLegal',
    'pcDireccion',
    'pcTelefono',
    'pcEmail',
    'pcEstado',
  ];
  protected readonly pageSizeOptions = [10, 25, 50];

  protected readonly rows = signal<QuarantineRow[]>([]);
  protected readonly loading = signal(false);
  /** La última carga falló: se ofrece reintentar en lugar del estado vacío. */
  protected readonly loadFailed = signal(false);

  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(this.pageSizeOptions[0]);

  /** Solo se renderizan las filas de la página actual. */
  protected readonly pageRows = computed(() => {
    const start = this.pageIndex() * this.pageSize();
    return this.rows().slice(start, start + this.pageSize());
  });

  constructor() {
    this.load();
  }

  /** Consulta la cuarentena. Se llama una vez al entrar y al pulsar "Actualizar". */
  protected load(): void {
    if (this.loading()) {
      return;
    }

    this.loading.set(true);
    this.loadFailed.set(false);

    this.api
      .getQuarantine()
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (records) => {
          this.rows.set(records.map((record) => ({ ...record, reasons: record.motivo.split(REASON_SEPARATOR) })));
          this.pageIndex.set(0);
        },
        // El mensaje lo muestra el interceptor global
        error: () => this.loadFailed.set(true),
      });
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }
}
