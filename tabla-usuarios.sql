-- Ejecutar en SSMS sobre la base AdopcionBd (F5). Es seguro correrlo más de una vez.
USE AdopcionBd;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE [Usuarios] (
        [UsuarioId]      INT IDENTITY(1,1) PRIMARY KEY,
        [NombreUsuario]  NVARCHAR(50)  NOT NULL,
        [Correo]         NVARCHAR(100) NOT NULL,
        [ContrasenaHash] NVARCHAR(200) NOT NULL,
        [Rol]            NVARCHAR(20)  NOT NULL,
        [FechaRegistro]  DATETIME2     NOT NULL,
        CONSTRAINT [UQ_Usuarios_NombreUsuario] UNIQUE ([NombreUsuario]),
        CONSTRAINT [UQ_Usuarios_Correo] UNIQUE ([Correo])
    );
END
GO

SELECT UsuarioId, NombreUsuario, Correo, Rol, FechaRegistro FROM Usuarios;
