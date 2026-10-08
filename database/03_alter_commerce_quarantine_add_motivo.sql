/*
  03 - Modifica commerce_quarantine agregando la columna motivo: el motivo por
  el cual el registro fue rechazado al procesar (si son varios, unidos por ' | ').
  Re-ejecutable: la columna se agrega solo si no existe.

  El valor por defecto '' permite agregar una columna NOT NULL aunque la tabla
  ya tenga filas.
*/
USE CommerceDb;
GO

IF COL_LENGTH(N'dbo.commerce_quarantine', N'motivo') IS NULL
BEGIN
    ALTER TABLE dbo.commerce_quarantine
        ADD motivo NVARCHAR(500) NOT NULL
            CONSTRAINT DF_commerce_quarantine_motivo DEFAULT N'';
END
GO
