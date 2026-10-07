import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';

import { errorInterceptor, toErrorMessage } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    snackBar = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBar },
      ],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('muestra el detalle del ProblemDetails y propaga el error', () => {
    let received: HttpErrorResponse | undefined;

    http.get('/api/x').subscribe({ error: (e: HttpErrorResponse) => (received = e) });
    controller
      .expectOne('/api/x')
      .flush({ detail: 'El archivo está vacío.' }, { status: 400, statusText: 'Bad Request' });

    expect(snackBar.open).toHaveBeenCalledOnceWith('El archivo está vacío.', 'Cerrar', jasmine.any(Object));
    expect(received?.status).toBe(400);
  });

  it('no muestra avisos cuando la respuesta es correcta', () => {
    http.get('/api/x').subscribe();
    controller.expectOne('/api/x').flush({});

    expect(snackBar.open).not.toHaveBeenCalled();
  });
});

describe('toErrorMessage', () => {
  it('avisa de falta de conexión cuando el estado es 0', () => {
    const message = toErrorMessage(new HttpErrorResponse({ status: 0 }));

    expect(message).toContain('No se pudo conectar');
  });

  it('usa un mensaje genérico si la respuesta no trae detalle', () => {
    const message = toErrorMessage(new HttpErrorResponse({ status: 500, error: null }));

    expect(message).toContain('error inesperado');
  });
});
