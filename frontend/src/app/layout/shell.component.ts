import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

interface NavLink {
  path: string;
  label: string;
  icon: string;
}

/** Estructura común de la aplicación: barra superior con navegación y área de contenido. */
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  protected readonly links: readonly NavLink[] = [
    { path: '/carga', label: 'Carga', icon: 'upload_file' },
    { path: '/procesar', label: 'Procesar', icon: 'play_circle' },
    { path: '/cuarentena', label: 'Cuarentena', icon: 'report' },
  ];
}
