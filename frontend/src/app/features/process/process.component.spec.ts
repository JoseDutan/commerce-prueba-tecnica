import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { CommerceApiService } from '../../core/services/commerce-api.service';
import { ProcessComponent } from './process.component';

describe('ProcessComponent', () => {
  let fixture: ComponentFixture<ProcessComponent>;
  let api: jasmine.SpyObj<CommerceApiService>;

  const element = (): HTMLElement => fixture.nativeElement as HTMLElement;

  const submitDate = (value: string): void => {
    const input = element().querySelector<HTMLInputElement>('input[type="date"]')!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    element().querySelector('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  };

  beforeEach(async () => {
    api = jasmine.createSpyObj<CommerceApiService>('CommerceApiService', ['process']);

    await TestBed.configureTestingModule({
      imports: [ProcessComponent],
      providers: [provideRouter([]), provideNoopAnimations(), { provide: CommerceApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(ProcessComponent);
    fixture.detectChanges();
  });

  it('no llama a la API si no hay fecha', () => {
    submitDate('');

    expect(api.process).not.toHaveBeenCalled();
    expect(element().textContent).toContain('Seleccione una fecha de proceso.');
  });

  it('procesa la fecha una sola vez y muestra los registros en cuarentena', () => {
    api.process.and.returnValue(of({ processDate: '2026-10-06', quarantinedCount: 9 }));

    submitDate('2026-10-06');

    expect(api.process).toHaveBeenCalledOnceWith('2026-10-06');
    expect(element().textContent).toContain('06/10/2026');
    expect(element().textContent).toContain('9 registros enviados a cuarentena');
  });

  it('informa cuando no hay registros con errores', () => {
    api.process.and.returnValue(of({ processDate: '2026-10-06', quarantinedCount: 0 }));

    submitDate('2026-10-06');

    expect(element().textContent).toContain('no se enviaron registros a cuarentena');
  });
});
