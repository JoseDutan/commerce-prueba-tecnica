/*
  05 - sp_process_commerce: valida los comercios de una fecha de proceso.

  Reglas (los valores con solo espacios cuentan como vacíos):
    - pc_nomcomred no vacío.
    - pc_numdoc no vacío, sin letras ni caracteres especiales (solo dígitos).

  Los registros que incumplen alguna regla se eliminan de commerce y se
  insertan en commerce_quarantine con todos sus motivos unidos por ' | '.

  Basado en conjuntos: un único DELETE ... OUTPUT INTO mueve las filas, sin
  cursores ni bucles. Es idempotente: al reprocesar la misma fecha ya no
  quedan filas inválidas en commerce, por lo que no se duplica la cuarentena.

  Devuelve: quarantined_count (filas insertadas en commerce_quarantine).
*/
USE CommerceDb;
GO

CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    -- Cualquier error revierte la transacción completa
    SET XACT_ABORT ON;

    DECLARE @quarantined INT;

    BEGIN TRANSACTION;

    DELETE c
    OUTPUT
        deleted.pc_processdate, deleted.pc_tipdoc, deleted.pc_numdoc,
        deleted.pc_nomcomred, deleted.pc_nomcomlegal, deleted.pc_direccion,
        deleted.pc_telefono, deleted.pc_email, deleted.pc_estado,
        v.motivo
    INTO dbo.commerce_quarantine
        (pc_processdate, pc_tipdoc, pc_numdoc, pc_nomcomred, pc_nomcomlegal,
         pc_direccion, pc_telefono, pc_email, pc_estado, motivo)
    FROM dbo.commerce AS c
    -- Valores normalizados: NULL y solo espacios pasan a ''
    CROSS APPLY (SELECT TRIM(ISNULL(c.pc_nomcomred, N'')) AS nomcomred,
                        TRIM(ISNULL(c.pc_numdoc, N''))    AS numdoc) AS t
    -- CONCAT_WS omite los NULL: solo quedan los motivos que aplican
    CROSS APPLY (SELECT CONCAT_WS(N' | ',
            CASE WHEN t.nomcomred = N''
                 THEN N'El nombre del comercio (nomcomred) se encuentra vacio' END,
            CASE WHEN t.numdoc = N''
                 THEN N'El número (numdoc) se encuentra vacio' END,
            CASE WHEN t.numdoc LIKE N'%[A-Za-z]%'
                 THEN N'El número (numdoc) contiene letras' END,
            CASE WHEN t.numdoc LIKE N'%[^0-9A-Za-z]%'
                 THEN N'El número (numdoc) contiene caracteres especiales' END
        ) AS motivo) AS v
    WHERE c.pc_processdate = @processdate
      AND v.motivo <> N'';

    SET @quarantined = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT @quarantined AS quarantined_count;
END
GO
