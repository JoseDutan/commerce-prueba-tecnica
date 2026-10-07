/*
  01 - Crea la base de datos CommerceDb si no existe.
  Re-ejecutable: no hace nada si la base ya está creada.
*/
IF DB_ID(N'CommerceDb') IS NULL
BEGIN
    CREATE DATABASE CommerceDb;
END
GO
