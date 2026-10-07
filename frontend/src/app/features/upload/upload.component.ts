import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-upload',
  standalone: true,
  template: '<section class="page"><h1 class="page-title">Carga de archivo</h1></section>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UploadComponent {}
