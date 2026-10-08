/*
  07 - sp_get_commerce_quarantine: lista los comercios en cuarentena
  con su motivo, ordenados por fecha de proceso.
*/
USE CommerceDb;
GO

CREATE OR ALTER PROCEDURE dbo.sp_get_commerce_quarantine
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        q.id, q.pc_processdate, q.pc_tipdoc, q.pc_numdoc, q.pc_nomcomred,
        q.pc_nomcomlegal, q.pc_direccion, q.pc_telefono, q.pc_email,
        q.pc_estado, q.motivo
    FROM dbo.commerce_quarantine AS q
    ORDER BY q.pc_processdate, q.id;
END
GO
