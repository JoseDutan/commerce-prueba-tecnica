# Comercios — carga, proceso y cuarentena

Prueba técnica full stack. Permite cargar un archivo `commerce_DDMMYYYY.csv`, procesar los comercios de un día y consultar los registros rechazados con su motivo.

| Capa | Tecnología |
|---|---|
| Frontend | Angular 18 (standalone, signals), Angular Material |
| Backend | .NET 8 Web API, Dapper, CsvHelper, Swagger |
| Base de datos | SQL Server 2022, stored procedures |

```
/database   scripts SQL numerados y re-ejecutables + CSV de ejemplo
/backend    solución .NET (API + pruebas unitarias)
/frontend   aplicación Angular
```

## Requisitos

- .NET SDK 8 (`global.json` fija la versión)
- Node 20 o superior y npm
- SQL Server 2022 accesible en `localhost,1433` y la herramienta `sqlcmd`
- Chrome (solo para las pruebas del frontend)

SQL Server en Docker, si no lo tiene:

```bash
docker run -d --name sqlserver -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='<su contraseña>' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

## Puesta en marcha

### 1. Contraseña de SQL Server

La contraseña no está en ningún archivo versionado. Copie `.env.example` como `.env.local` (ignorado por git) y complete `SQL_SA_PASSWORD`.

```bash
cp .env.example .env.local
```

### 2. Base de datos

Desde la raíz del repositorio. Los scripts se pueden ejecutar las veces que haga falta.

```bash
set -a; source .env.local; set +a
for f in database/0*.sql; do SQLCMDPASSWORD="$SQL_SA_PASSWORD" sqlcmd -S localhost,1433 -U sa -C -b -i "$f"; done
```

Pruebas manuales de los stored procedures (opcional, limpian sus propios datos):

```bash
SQLCMDPASSWORD="$SQL_SA_PASSWORD" sqlcmd -S localhost,1433 -U sa -C -W -i database/99_manual_tests.sql
```

### 3. API

```bash
set -a; source .env.local; set +a
dotnet run --project backend/src/Commerce.Api
```

- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`

La API lee la contraseña de la clave de configuración `SQL_SA_PASSWORD`. También puede definirse con `dotnet user-secrets set SQL_SA_PASSWORD "<valor>"` en el proyecto `Commerce.Api`.

### 4. Frontend

```bash
cd frontend
npm install
npm start
```

Aplicación: `http://localhost:4200`

### Pruebas

```bash
dotnet test backend
```

```bash
cd frontend && npm test -- --watch=false --browsers=ChromeHeadless
```

## Uso

1. **Carga**: seleccione `database/samples/commerce_06102026.csv`, revise la previsualización y pulse Enviar.
2. **Procesar**: elija la fecha de proceso (`06/10/2026`) y pulse Procesar. Con el archivo de ejemplo, 9 registros pasan a cuarentena (2 para `07/10/2026`).
3. **Cuarentena**: consulte los registros rechazados y sus motivos.

## Endpoints

| Método | Ruta | Descripción | Respuesta |
|---|---|---|---|
| POST | `/api/commerce/upload` | Recibe el archivo en el campo `file` (`multipart/form-data`) y lo inserta en `commerce` | `{ fileName, insertedCount }` |
| POST | `/api/commerce/process` | Cuerpo `{ "processDate": "2026-10-06" }`. Mueve los registros inválidos a `commerce_quarantine` | `{ processDate, quarantinedCount }` |
| GET | `/api/commerce/quarantine` | Lista los comercios en cuarentena con su `motivo` | arreglo de registros |

Los errores se devuelven como `ProblemDetails` (RFC 7807): `400` para peticiones inválidas (archivo vacío, nombre incorrecto, CSV mal formado, fecha inválida) y `500` para errores inesperados, sin exponer detalles internos.

### Reglas de validación al procesar

| Regla | Motivo registrado |
|---|---|
| `pc_nomcomred` vacío | `El nombre del comercio (nomcomred) se encuentra vacio` |
| `pc_numdoc` vacío | `El número (numdoc) se encuentra vacio` |
| `pc_numdoc` con letras | `El número (numdoc) contiene letras` |
| `pc_numdoc` con caracteres especiales | `El número (numdoc) contiene caracteres especiales` |

Los valores con solo espacios cuentan como vacíos. Si un registro incumple varias reglas, se guardan todos los motivos unidos por ` | `.

## Decisiones técnicas

### Base de datos

- **Table-Valued Parameter en `sp_create_commerce`**: todas las filas del archivo viajan en una sola llamada y se insertan con un único `INSERT ... SELECT`, en lugar de un viaje de red por fila. El stored procedure sigue siendo el único punto de inserción.
- **Proceso basado en conjuntos**: `sp_process_commerce` usa un solo `DELETE ... OUTPUT INTO` que borra de `commerce` e inserta en `commerce_quarantine` en la misma sentencia atómica, sin cursores ni bucles. Los motivos se arman con `CONCAT_WS`, que omite los que no aplican.
- **Idempotencia**: al reprocesar una fecha ya no quedan registros inválidos en `commerce`, por lo que no se duplica la cuarentena.
- **Recarga por reemplazo**: volver a subir un archivo reemplaza, dentro de una transacción, las fechas de proceso que contiene (en `commerce` y en `commerce_quarantine`).
- **Identificadores como texto**: `pc_numdoc` y `pc_telefono` conservan ceros a la izquierda y permiten guardar los valores inválidos para validarlos.
- **Índice por `pc_processdate`** en ambas tablas, porque la carga y el proceso filtran por esa columna.

### Backend

- **Capas**: `Controllers` (HTTP) → `Services` (reglas de aplicación) → `Repositories` (stored procedures). Cada capa depende de interfaces registradas en el contenedor de inyección de dependencias.
- **Dapper** en lugar de Entity Framework: todo el acceso es por stored procedures y Dapper soporta TVP de forma directa.
- **Middleware global de errores**: convierte las excepciones en `ProblemDetails`; los controladores no repiten `try/catch`.
- **CSV configurable** (sección `Csv` de `appsettings.json`): delimitador y formatos de fecha. La lectura es UTF-8 estricta para no guardar acentos corruptos.
- **Todo asíncrono** con `CancellationToken`: si el cliente cancela, la consulta a la base también se cancela.
- **CORS** limitado a los orígenes de `Cors:AllowedOrigins`.

### Frontend

- **Componentes standalone con carga diferida**: cada pantalla se descarga solo al navegar a ella.
- **Signals y `OnPush`** para el estado de cada pantalla; RxJS se usa en las llamadas HTTP.
- **Un servicio HTTP tipado** (`CommerceApiService`) y un **interceptor global de errores** que muestra el mensaje enviado por la API.
- **Previsualización en el cliente** con PapaParse y tabla paginada: solo se dibujan las filas de la página actual. Se envía el archivo original; el backend lo vuelve a validar.
- **Sin llamadas repetidas**: una suscripción por acción del usuario, botones deshabilitados durante la petición y cancelación al salir de la pantalla.
- **Fechas como texto `yyyy-MM-dd`** de extremo a extremo, sin pasar por `Date`, para evitar desfases por zona horaria.

## Supuestos

1. El backend es .NET (la mención a SpringBoot del enunciado se consideró un error).
2. La fecha del nombre del archivo se valida (patrón y fecha real), pero no se exige que coincida con `pc_processdate`: un archivo puede traer varias fechas de proceso.
3. Estructura del CSV: UTF-8, separado por comas, con cabecera `pc_processdate,pc_tipdoc,pc_numdoc,pc_nomcomred,pc_nomcomlegal,pc_direccion,pc_telefono,pc_email,pc_estado`.
4. `commerce` tiene exactamente las columnas del CSV. `commerce_quarantine` añade `id` y `motivo`.
5. Un archivo con cabecera incorrecta, columnas faltantes, textos que exceden la longitud de la columna o `pc_processdate` no válido se rechaza completo con `400` y el detalle por línea. Las reglas de negocio (`nomcomred`, `numdoc`) no bloquean la carga: se evalúan al procesar.
6. `pc_numdoc` válido significa solo dígitos; un espacio intermedio cuenta como carácter especial y una letra acentuada cuenta como letra.
7. Procesar una fecha sin registros devuelve `200` con `quarantinedCount: 0`.

## Mejoras posibles

- Paginación y filtro por fecha en el servidor para `GET /quarantine` con grandes volúmenes.
- Pruebas de integración de los stored procedures y del repositorio contra una base de datos de pruebas.
- Autenticación (inicio de sesión), indicada como opcional en el enunciado.
- Identificador propio en `commerce` y registro de auditoría de cargas.
