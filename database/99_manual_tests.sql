/*
  99 - Pruebas manuales de los stored procedures.

  Usa la fecha 1900-01-01 para no tocar datos reales y limpia al terminar.
  Resultado esperado en cada bloque: columna "resultado" = OK.
*/
USE CommerceDb;
GO

SET NOCOUNT ON;

DECLARE @fecha DATE = '1900-01-01';
DECLARE @rows dbo.CommerceTableType;
DECLARE @conteo TABLE (n INT);

INSERT INTO @rows (pc_processdate, pc_tipdoc, pc_numdoc, pc_nomcomred) VALUES
    (@fecha, N'RUC', N'0991234567001', N'Válido'),          -- válido
    (@fecha, N'RUC', N'0994567890001', N''),                -- nombre vacío
    (@fecha, N'RUC', N'',              N'Sin número'),      -- numdoc vacío
    (@fecha, N'RUC', N'09956789ABC',   N'Con letras'),      -- letras
    (@fecha, N'RUC', N'0996789012-001',N'Con guion'),       -- carácter especial
    (@fecha, N'CED', N'0923456789',    N'   '),             -- nombre solo espacios
    (@fecha, N'RUC', N'   ',           N'Número en blanco'),-- numdoc solo espacios
    (@fecha, N'RUC', NULL,             NULL),               -- ambos NULL
    (@fecha, N'RUC', N'AB-12',         N'');                -- tres motivos

-- 1) Carga: inserta las 9 filas
INSERT INTO @conteo EXEC dbo.sp_create_commerce @rows = @rows;
SELECT '1 carga inserta 9' AS prueba,
       IIF((SELECT n FROM @conteo) = 9, 'OK', 'FALLA') AS resultado;

-- 2) Recarga: reemplaza, no duplica
DELETE FROM @conteo;
INSERT INTO @conteo EXEC dbo.sp_create_commerce @rows = @rows;
SELECT '2 recarga no duplica' AS prueba,
       IIF((SELECT COUNT(*) FROM dbo.commerce WHERE pc_processdate = @fecha) = 9, 'OK', 'FALLA') AS resultado;

-- 3) Procesar: 8 a cuarentena, 1 válido permanece
DELETE FROM @conteo;
INSERT INTO @conteo EXEC dbo.sp_process_commerce @processdate = @fecha;
SELECT '3 procesar devuelve 8' AS prueba,
       IIF((SELECT n FROM @conteo) = 8, 'OK', 'FALLA') AS resultado;
SELECT '3 queda 1 en commerce' AS prueba,
       IIF((SELECT COUNT(*) FROM dbo.commerce WHERE pc_processdate = @fecha) = 1, 'OK', 'FALLA') AS resultado;

-- 4) Idempotencia: reprocesar devuelve 0 y la cuarentena sigue en 8
DELETE FROM @conteo;
INSERT INTO @conteo EXEC dbo.sp_process_commerce @processdate = @fecha;
SELECT '4 reprocesar devuelve 0' AS prueba,
       IIF((SELECT n FROM @conteo) = 0
           AND (SELECT COUNT(*) FROM dbo.commerce_quarantine WHERE pc_processdate = @fecha) = 8,
           'OK', 'FALLA') AS resultado;

-- 5) Varios motivos en un mismo registro
SELECT '5 registro con 3 motivos' AS prueba,
       IIF(EXISTS (SELECT 1 FROM dbo.commerce_quarantine
                   WHERE pc_processdate = @fecha AND pc_numdoc = N'AB-12'
                     AND motivo = N'El nombre del comercio (nomcomred) se encuentra vacio | El número (numdoc) contiene letras | El número (numdoc) contiene caracteres especiales'),
           'OK', 'FALLA') AS resultado;

-- Detalle para revisión visual
SELECT pc_numdoc, pc_nomcomred, motivo
FROM dbo.commerce_quarantine
WHERE pc_processdate = @fecha
ORDER BY id;

-- Limpieza
DELETE FROM dbo.commerce WHERE pc_processdate = @fecha;
DELETE FROM dbo.commerce_quarantine WHERE pc_processdate = @fecha;
GO
