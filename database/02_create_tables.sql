/*
  02 - Tablas commerce y commerce_quarantine.
  Re-ejecutable: cada objeto se crea solo si no existe.

  Los identificadores (pc_numdoc, pc_telefono) son texto: tienen ceros a la
  izquierda y deben poder guardar valores inválidos para validarlos después.
*/
USE CommerceDb;
GO

-- Misma estructura que el archivo commerce_DDMMYYYY.csv
IF OBJECT_ID(N'dbo.commerce', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce
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

-- Registros rechazados al procesar: mismas columnas que commerce.
-- La columna motivo se agrega en 03_alter_commerce_quarantine_add_motivo.sql
IF OBJECT_ID(N'dbo.commerce_quarantine', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_quarantine
    (
        id             INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_commerce_quarantine PRIMARY KEY,
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

-- Carga y proceso filtran siempre por fecha de proceso
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_commerce_processdate'
                 AND object_id = OBJECT_ID(N'dbo.commerce'))
BEGIN
    CREATE INDEX IX_commerce_processdate ON dbo.commerce (pc_processdate);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_commerce_quarantine_processdate'
                 AND object_id = OBJECT_ID(N'dbo.commerce_quarantine'))
BEGIN
    CREATE INDEX IX_commerce_quarantine_processdate ON dbo.commerce_quarantine (pc_processdate);
END
GO
