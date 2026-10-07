import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ProcessResult } from '../../core/models/commerce.models';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { IsoDatePipe } from '../../shared/pipes/iso-date.pipe';

/**
 * Pantalla de proceso: el usuario elige una fecha de proceso (pc_processdate)
 * y la API valida los comercios de ese día, enviando los inválidos a cuarentena.
 */
@Component({
  selector: 'app-process',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    IsoDatePipe,
  ],
  templateUrl: './process.component.html',
  styleUrl: './process.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProcessComponent {
  private readonly api = inject(CommerceApiService);
  private readonly destroyRef = inject(DestroyRef);

  /** El input type="date" entrega el valor ya en formato yyyy-MM-dd, el que espera la API. */
  protected readonly form = new FormGroup({
    processDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly processing = signal(false);
  protected readonly result = signal<ProcessResult | null>(null);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (this.processing()) {
      return;
    }

    this.processing.set(true);
    this.result.set(null);

    this.api
      .process(this.form.controls.processDate.value)
      .pipe(
        finalize(() => this.processing.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      // Los errores los notifica el interceptor global
      .subscribe({ next: (result) => this.result.set(result), error: () => undefined });
  }
}
