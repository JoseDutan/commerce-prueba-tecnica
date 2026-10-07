import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-quarantine',
  standalone: true,
  template: '<section class="page"><h1 class="page-title">Cuarentena</h1></section>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuarantineComponent {}
