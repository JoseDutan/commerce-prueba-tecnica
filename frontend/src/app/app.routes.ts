import { Routes } from '@angular/router';

import { ShellComponent } from './layout/shell.component';

/**
 * Cada pantalla se carga de forma diferida (lazy loading): su código solo se
 * descarga cuando el usuario navega a ella.
 */
export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'carga' },
      {
        path: 'carga',
        title: 'Carga de archivo | Comercios',
        loadComponent: () =>
          import('./features/upload/upload.component').then((m) => m.UploadComponent),
      },
      {
        path: 'procesar',
        title: 'Procesar | Comercios',
        loadComponent: () =>
          import('./features/process/process.component').then((m) => m.ProcessComponent),
      },
      {
        path: 'cuarentena',
        title: 'Cuarentena | Comercios',
        loadComponent: () =>
          import('./features/quarantine/quarantine.component').then((m) => m.QuarantineComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
