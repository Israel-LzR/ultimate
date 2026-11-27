-- Tabla donde se guarda el dinero con el que ya cuenta el refugio (saldo inicial: $87,000).
-- El fondo total que ve el administrador = SaldoInicial + suma de las donaciones confirmadas.
-- Ejecútalo UNA vez en SSMS (se puede repetir sin problema).
USE AdopcionBd;
GO

IF OBJECT_ID('dbo.Fondos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Fondos (
        FondoId            INT IDENTITY(1,1) PRIMARY KEY,
        SaldoInicial       DECIMAL(18,2) NOT NULL,
        FechaActualizacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Fondos)
    INSERT INTO dbo.Fondos (SaldoInicial) VALUES (87000.00);
GO

-- Para cambiar el saldo inicial más adelante:
-- UPDATE dbo.Fondos SET SaldoInicial = 90000, FechaActualizacion = SYSDATETIME();

-- Ver el fondo total (saldo inicial + donaciones confirmadas):
SELECT f.SaldoInicial,
       ISNULL(SUM(d.Monto), 0)                     AS TotalDonado,
       f.SaldoInicial + ISNULL(SUM(d.Monto), 0)    AS FondoTotal
FROM dbo.Fondos f
LEFT JOIN dbo.Donaciones d ON d.Estado = 'Confirmada'
GROUP BY f.SaldoInicial;
GO
