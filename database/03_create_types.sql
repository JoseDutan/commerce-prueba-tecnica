/*
  03 - Tipo de tabla (Table-Valued Parameter) para enviar todas las filas
  del CSV a sp_create_commerce en una sola llamada.
  Re-ejecutable: el tipo se crea solo si no existe.

  El orden y los tipos de las columnas deben coincidir con el DataTable
  que construye la API.
*/
USE CommerceDb;
GO

IF TYPE_ID(N'dbo.CommerceTableType') IS NULL
BEGIN
    CREATE TYPE dbo.CommerceTableType AS TABLE
    (
        pc_processdate DATE          NOT NULL,
        pc_tipdoc      NVARCHAR(10)  NULL,
        pc_numdoc      NVARCHAR(50)  NULL,
        pc_nomcomred   NVARCHAR(200) NULL,
        pc_nomcomlegal NVARCHAR(200) NULL,
        pc_direccion   NVARCHAR(300) NULL,
        pc_telefono    NVARCHAR(30)  NULL,
        pc_email       NVARCHAR(150) NULL,
        pc_estado      NVARCHAR(20)  NULL
    );
END
GO
