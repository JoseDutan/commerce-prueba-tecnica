import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { QuarantineRecord } from '../../core/models/commerce.models';
import { CommerceApiService } from '../../core/services/commerce-api.service';
import { QuarantineComponent } from './quarantine.component';

describe('QuarantineComponent', () => {
  let api: jasmine.SpyObj<CommerceApiService>;

  const record: QuarantineRecord = {
    id: 1,
    pcProcessDate: '2026-10-06',
    pcTipDoc: 'RUC',
    pcNumDoc: 'ABC123',
    pcNomComRed: '',
    pcNomComLegal: 'Otro Doble Error S.A.',
    pcDireccion: 'Calle Luque 200',
    pcTelefono: '042003344',
    pcEmail: 'doble2@mail.com',
    pcEstado: 'ACTIVO',
    motivo: 'El nombre del comercio (nomcomred) se encuentra vacio | El número (numdoc) contiene letras',
  };

  const create = (): ComponentFixture<QuarantineComponent> => {
    const fixture = TestBed.createComponent(QuarantineComponent);
    fixture.detectChanges();
    return fixture;
  };

  const text = (fixture: ComponentFixture<QuarantineComponent>): string =>
    (fixture.nativeElement as HTMLElement).textContent ?? '';

  beforeEach(async () => {
    api = jasmine.createSpyObj<CommerceApiService>('CommerceApiService', ['getQuarantine']);

    await TestBed.configureTestingModule({
      imports: [QuarantineComponent],
      providers: [provideNoopAnimations(), { provide: CommerceApiService, useValue: api }],
    }).compileComponents();
  });

  it('consulta la cuarentena una sola vez al entrar y muestra cada motivo', () => {
    api.getQuarantine.and.returnValue(of([record]));

    const fixture = create();

    expect(api.getQuarantine).toHaveBeenCalledTimes(1);
    const reasons = (fixture.nativeElement as HTMLElement).querySelectorAll('.reasons li');
    expect(reasons.length).toBe(2);
    expect(text(fixture)).toContain('06/10/2026');
    expect(text(fixture)).toContain('(vacío)');
  });

  it('muestra el estado vacío cuando no hay registros', () => {
    api.getQuarantine.and.returnValue(of([]));

    const fixture = create();

    expect(text(fixture)).toContain('No hay comercios en cuarentena.');
  });

  it('ofrece reintentar cuando la carga falla', () => {
    api.getQuarantine.and.returnValue(throwError(() => new Error('sin conexión')));

    const fixture = create();

    expect(text(fixture)).toContain('No se pudo cargar la cuarentena.');
  });
});
