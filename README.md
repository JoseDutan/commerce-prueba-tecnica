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

| Herramienta | Versión | Para qué |
|---|---|---|
| .NET SDK | 8.0 (`global.json` fija 8.0.x) | API |
| Node.js y npm | 20 o superior | Frontend |
| SQL Server | 2019 o superior, accesible en `localhost,1433` | Base de datos |
| `sqlcmd` o SQL Server Management Studio | cualquiera | Ejecutar los scripts |
| Google Chrome | cualquiera | Solo para las pruebas del frontend |

Si no tiene SQL Server instalado, puede levantarlo con Docker:

```bash
docker run -d --name sqlserver -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='<su contraseña>' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

## Puesta en marcha

Son cuatro pasos: contraseña, base de datos, API y frontend. Todos los comandos se ejecutan **desde la raíz del repositorio**. Se necesitan dos terminales: una para la API y otra para el frontend.

### Paso 1. Definir la contraseña de SQL Server

La contraseña del usuario `sa` no está en ningún archivo del repositorio. Se toma de la variable de entorno `SQL_SA_PASSWORD`, que debe definirse en cada terminal que se use para la base de datos o la API.

macOS / Linux:

```bash
export SQL_SA_PASSWORD='<su contraseña>'
```

Windows (PowerShell):

```powershell
$env:SQL_SA_PASSWORD = '<su contraseña>'
```

> Si usa otra instancia, otro usuario o autenticación de Windows, vea [Usar otra conexión](#usar-otra-conexión).

### Paso 2. Crear la base de datos

Los scripts de `/database` crean la base `CommerceDb`, las tablas y los stored procedures. Están numerados, deben ejecutarse en orden y pueden repetirse sin riesgo.

macOS / Linux:

```bash
for f in database/0*.sql; do SQLCMDPASSWORD="$SQL_SA_PASSWORD" sqlcmd -S localhost,1433 -U sa -C -b -i "$f"; done
```

Windows (PowerShell):

```powershell
Get-ChildItem database\0*.sql | Sort-Object Name | ForEach-Object { sqlcmd -S localhost,1433 -U sa -P $env:SQL_SA_PASSWORD -C -b -i $_.FullName }
```

Alternativa sin línea de comandos: abra en SQL Server Management Studio los archivos `01` a `07` de la carpeta `database` y ejecútelos uno por uno, en orden.

| Script | Contenido |
|---|---|
| `01_create_database.sql` | Base de datos `CommerceDb` |
| `02_create_tables.sql` | Tablas `commerce` y `commerce_quarantine` |
| `03_alter_commerce_quarantine_add_motivo.sql` | Agrega la columna `motivo` a `commerce_quarantine` |
| `04_create_types.sql` | Tipo de tabla usado por la carga |
| `05_sp_create_commerce.sql` | Inserción de las filas del archivo |
| `06_sp_process_commerce.sql` | Validación y envío a cuarentena |
| `07_sp_get_commerce_quarantine.sql` | Listado de la cuarentena |
| `99_manual_tests.sql` | Pruebas manuales opcionales (no forman parte de la instalación) |

### Paso 3. Levantar la API

En la misma terminal donde definió `SQL_SA_PASSWORD`:

```bash
dotnet run --project backend/src/Commerce.Api
```

Cuando aparezca `Now listening on: http://localhost:5080`, la API está lista.

- Swagger: <http://localhost:5080/swagger>

### Paso 4. Levantar el frontend

En una segunda terminal:

```bash
cd frontend
npm install
npm start
```

- Aplicación: <http://localhost:4200>

## Probar la aplicación

El repositorio incluye un archivo de ejemplo: `database/samples/commerce_06102026.csv` (19 registros, 11 de ellos con errores).

1. **Carga**: pulse *Seleccionar archivo*, elija el archivo de ejemplo, revise la previsualización y pulse *Enviar*. Resultado esperado: 19 registros insertados.
2. **Procesar**: elija la fecha `06/10/2026` y pulse *Procesar*. Resultado esperado: 9 registros enviados a cuarentena. Con `07/10/2026`: 2 registros.
3. **Cuarentena**: se listan los 11 registros rechazados con sus motivos.

Volver a procesar la misma fecha devuelve 0 y no duplica registros.

## Pruebas automáticas

Backend (27 pruebas):

```bash
dotnet test backend
```

Frontend (38 pruebas):

```bash
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
```

Stored procedures (script manual; cada línea debe mostrar `OK` y limpia sus propios datos):

```bash
SQLCMDPASSWORD="$SQL_SA_PASSWORD" sqlcmd -S localhost,1433 -U sa -C -W -i database/99_manual_tests.sql
```

## Usar otra conexión

Por defecto la API se conecta a `localhost,1433` con el usuario `sa` y la contraseña de `SQL_SA_PASSWORD`. Para usar otra instancia, otro usuario o autenticación de Windows, defina la cadena de conexión completa en la variable `ConnectionStrings__CommerceDb` (con doble guion bajo) antes de levantar la API. En ese caso `SQL_SA_PASSWORD` no es necesaria.

macOS / Linux:

```bash
export ConnectionStrings__CommerceDb='Server=<servidor>;Database=CommerceDb;User Id=<usuario>;Password=<contraseña>;TrustServerCertificate=True'
```

Windows (PowerShell), con autenticación de Windows:

```powershell
$env:ConnectionStrings__CommerceDb = 'Server=localhost\SQLEXPRESS;Database=CommerceDb;Integrated Security=True;TrustServerCertificate=True'
```

Al ejecutar los scripts con `sqlcmd`, cambie también `-S`, `-U` y `-P` por los de su instancia (o use `-E` para autenticación de Windows).

## Problemas frecuentes

| Síntoma | Causa y solución |
|---|---|
| La aplicación muestra "No se pudo conectar con el servidor" | La API no está en ejecución. Levántela (paso 3) y verifique <http://localhost:5080/swagger>. |
| La API responde "Error interno del servidor" | No puede conectarse a la base. Revise que SQL Server esté arriba, que `SQL_SA_PASSWORD` esté definida en la terminal de la API y que se hayan ejecutado los scripts. El detalle aparece en la consola de la API. |
| `Login failed for user 'sa'` | La contraseña de `SQL_SA_PASSWORD` no coincide con la de SQL Server. |
| `sqlcmd` no reconoce la opción `-C` | Es una versión antigua. Quite `-C` o ejecute los scripts desde SQL Server Management Studio. |
| El puerto 5080 o 4200 está ocupado | Cierre el proceso que lo usa. Si cambia el puerto de la API, actualice `frontend/src/environments/environment.development.ts`. |
| Error al subir un archivo propio | El nombre debe ser `commerce_DDMMYYYY.csv` con una fecha real, en UTF-8, separado por comas y con la cabecera indicada en [Supuestos](#supuestos). |

## Endpoints

| Método | Ruta | Descripción | Respuesta |
|---|---|---|---|
| POST | `/api/commerce/upload` | Recibe el archivo en el campo `file` (`multipart/form-data`) y lo inserta en `commerce` | `{ fileName, insertedCount }` |
| POST | `/api/commerce/process` | Cuerpo `{ "processDate": "2026-10-06" }`. Mueve los registros inválidos a `commerce_quarantine` | `{ processDate, quarantinedCount }` |
| GET | `/api/commerce/quarantine` | Lista los comercios en cuarentena con su `motivo` | arreglo de registros |

Los errores se devuelven como `ProblemDetails` (RFC 7807): `400` para peticiones inválidas (archivo vacío, nombre incorrecto, CSV mal formado, fecha inválida) y `500` para errores inesperados, sin exponer detalles internos.

Ejemplos con `curl`:

```bash
curl -F "file=@database/samples/commerce_06102026.csv" http://localhost:5080/api/commerce/upload
```

```bash
curl -H "Content-Type: application/json" -d '{"processDate":"2026-10-06"}' http://localhost:5080/api/commerce/process
```

```bash
curl http://localhost:5080/api/commerce/quarantine
```

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
- **Sin secretos versionados**: la cadena de conexión del repositorio no lleva contraseña.

### Frontend

- **Componentes standalone con carga diferida**: cada pantalla se descarga solo al navegar a ella.
- **Signals y `OnPush`** para el estado de cada pantalla; RxJS se usa en las llamadas HTTP.
- **Un servicio HTTP tipado** (`CommerceApiService`) y un **interceptor global de errores** que muestra el mensaje enviado por la API.
- **Previsualización en el cliente** con PapaParse y tabla paginada: solo se dibujan las filas de la página actual. Se envía el archivo original; el backend lo vuelve a validar.
- **Sin llamadas repetidas**: una suscripción por acción del usuario, botones deshabilitados durante la petición y cancelación al salir de la pantalla. El flujo completo (cargar, procesar, consultar) son tres peticiones.
- **Fechas como texto `yyyy-MM-dd`** de extremo a extremo, sin pasar por `Date`, para evitar desfases por zona horaria.

## Supuestos

1. El backend es .NET (la mención a SpringBoot del enunciado se consideró un error).
2. La fecha del nombre del archivo se valida (patrón y fecha real), pero no se exige que coincida con `pc_processdate`: un archivo puede traer varias fechas de proceso.
3. Estructura del CSV: UTF-8, separado por comas, con cabecera `pc_processdate,pc_tipdoc,pc_numdoc,pc_nomcomred,pc_nomcomlegal,pc_direccion,pc_telefono,pc_email,pc_estado` y fechas en formato `yyyy-MM-dd` o `dd/MM/yyyy`.
4. `commerce` tiene exactamente las columnas del CSV. `commerce_quarantine` se crea con esas mismas columnas más un `id`, y la columna `motivo` se agrega con un `ALTER TABLE` en `03_alter_commerce_quarantine_add_motivo.sql`, como indica el enunciado.
5. Un archivo con cabecera incorrecta, columnas faltantes, textos que exceden la longitud de la columna o `pc_processdate` no válido se rechaza completo con `400` y el detalle por línea. Las reglas de negocio (`nomcomred`, `numdoc`) no bloquean la carga: se evalúan al procesar.
6. `pc_numdoc` válido significa solo dígitos; un espacio intermedio cuenta como carácter especial y una letra acentuada cuenta como letra.
7. Procesar una fecha sin registros devuelve `200` con `quarantinedCount: 0`.

## Mejoras posibles

- Paginación y filtro por fecha en el servidor para `GET /quarantine` con grandes volúmenes.
- Pruebas de integración de los stored procedures y del repositorio contra una base de datos de pruebas.
- Autenticación (inicio de sesión), indicada como opcional en el enunciado.
- Identificador propio en `commerce` y registro de auditoría de cargas.
