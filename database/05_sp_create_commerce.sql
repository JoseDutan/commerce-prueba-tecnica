/*
  05 - sp_create_commerce: inserta en commerce las filas de un archivo.

  Recibe todas las filas en un Table-Valued Parameter: una sola llamada y un
  solo INSERT basado en conjuntos, en lugar de un round-trip por fila.

  Recarga: reemplaza por fecha. Antes de insertar elimina de commerce y de
  commerce_quarantine las fechas de proceso que trae el archivo, de modo que
  volver a subir el mismo archivo no duplica registros.

  Devuelve: inserted_count (filas insertadas en commerce).
*/
USE CommerceDb;
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @rows dbo.CommerceTableType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- Cualquier error revierte la transacción completa
    SET XACT_ABORT ON;

    DECLARE @inserted INT;

    BEGIN TRANSACTION;

    DELETE c
    FROM dbo.commerce AS c
    WHERE c.pc_processdate IN (SELECT r.pc_processdate FROM @rows AS r);

    DELETE q
    FROM dbo.commerce_quarantine AS q
    WHERE q.pc_processdate IN (SELECT r.pc_processdate FROM @rows AS r);

    INSERT INTO dbo.commerce
        (pc_processdate, pc_tipdoc, pc_numdoc, pc_nomcomred, pc_nomcomlegal,
         pc_direccion, pc_telefono, pc_email, pc_estado)
    SELECT
        r.pc_processdate, r.pc_tipdoc, r.pc_numdoc, r.pc_nomcomred, r.pc_nomcomlegal,
        r.pc_direccion, r.pc_telefono, r.pc_email, r.pc_estado
    FROM @rows AS r;

    SET @inserted = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT @inserted AS inserted_count;
END
GO
