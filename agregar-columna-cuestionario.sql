-- Agrega a la tabla Solicitudes la columna donde se guarda el resultado del cuestionario
-- de aptitud ("Apto 88%"). Ejecútalo UNA vez en SSMS (se puede repetir sin problema).
USE AdopcionBd;
GO

IF COL_LENGTH('dbo.Solicitudes', 'ResultadoCuestionario') IS NULL
    ALTER TABLE dbo.Solicitudes ADD ResultadoCuestionario NVARCHAR(40) NULL;
GO

-- Comprobar:
SELECT TOP 10 * FROM dbo.Solicitudes;
