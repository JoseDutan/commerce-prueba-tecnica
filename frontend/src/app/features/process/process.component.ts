import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-process',
  standalone: true,
  template: '<section class="page"><h1 class="page-title">Procesar</h1></section>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProcessComponent {}
