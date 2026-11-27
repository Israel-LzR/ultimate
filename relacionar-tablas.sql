-- =====================================================================
--  Adopi - Deja TODA la base AdopcionBd completa y con todas las tablas relacionadas
--  Ejecútalo en SSMS (F5) ANTES de correr el proyecto. Se puede repetir sin problema.
--
--  Tablas:  Animales, Fotos, Solicitudes, Donaciones, Usuarios, Fondos
--  Relaciones (llaves foráneas) al terminar:
--     Fotos        .AnimalId  -> Animales.AnimalId      (borrar animal borra sus fotos)
--     Solicitudes  .AnimalId  -> Animales.AnimalId      (borrar animal borra sus solicitudes)
--     Solicitudes  .UsuarioId -> Usuarios.UsuarioId     (quién envió la solicitud)
--     Donaciones   .UsuarioId -> Usuarios.UsuarioId     (quién donó; vacío si donó sin cuenta)
--     Donaciones   .FondoId   -> Fondos.FondoId         (a qué fondo se suma la donación)
--
--  Este script ya incluye lo de agregar-columna-cuestionario.sql, crear-tabla-fondo.sql
--  y tabla-usuarios.sql, así que con correr solo este queda todo.
-- =====================================================================
USE AdopcionBd;
GO

-- ---------------------------------------------------------------------
-- 1) Tablas y columnas que faltaban
-- ---------------------------------------------------------------------

-- Usuarios (cuentas del sitio)
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioId      INT IDENTITY(1,1) PRIMARY KEY,
        NombreUsuario  NVARCHAR(50)  NOT NULL,
        Correo         NVARCHAR(100) NOT NULL,
        ContrasenaHash NVARCHAR(200) NOT NULL,
        Rol            NVARCHAR(20)  NOT NULL,
        FechaRegistro  DATETIME2     NOT NULL,
        CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo)
    );
END
GO

-- Fondos (dinero con el que ya cuenta el refugio: 87,000 al inicio)
IF OBJECT_ID(N'dbo.Fondos', N'U') IS NULL
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

-- Donaciones (ya la tienes; solo se crea si por alguna razón no existe)
IF OBJECT_ID(N'dbo.Donaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Donaciones (
        DonacionId    INT IDENTITY(1,1) PRIMARY KEY,
        NombreDonante NVARCHAR(80)  NOT NULL,
        Monto         DECIMAL(18,2) NOT NULL,
        Moneda        NVARCHAR(10)  NOT NULL,
        Fecha         DATETIME2     NOT NULL,
        IdTransaccion NVARCHAR(50)  NULL,
        Estado        NVARCHAR(30)  NOT NULL
    );
END
GO

-- Resultado del cuestionario de aptitud (ej. "Apto 88%") en cada solicitud
IF COL_LENGTH(N'dbo.Solicitudes', N'ResultadoCuestionario') IS NULL
    ALTER TABLE dbo.Solicitudes ADD ResultadoCuestionario NVARCHAR(40) NULL;
GO

-- Columnas nuevas para las relaciones (aceptan NULL: hay solicitudes/donaciones viejas y se puede donar sin cuenta)
IF COL_LENGTH(N'dbo.Solicitudes', N'UsuarioId') IS NULL
    ALTER TABLE dbo.Solicitudes ADD UsuarioId INT NULL;
GO

IF COL_LENGTH(N'dbo.Donaciones', N'UsuarioId') IS NULL
    ALTER TABLE dbo.Donaciones ADD UsuarioId INT NULL;
GO

IF COL_LENGTH(N'dbo.Donaciones', N'FondoId') IS NULL
    ALTER TABLE dbo.Donaciones ADD FondoId INT NULL;
GO

-- ---------------------------------------------------------------------
-- 2) Llenar las columnas nuevas con los datos que ya tienes (no se borra nada)
-- ---------------------------------------------------------------------

-- Cada solicitud vieja se liga a la cuenta que tenga ese mismo correo
UPDATE s
SET    s.UsuarioId = u.UsuarioId
FROM   dbo.Solicitudes s
JOIN   dbo.Usuarios u ON u.Correo = s.Correo
WHERE  s.UsuarioId IS NULL;
GO

-- Toda donación vieja se liga al fondo del refugio
UPDATE dbo.Donaciones
SET    FondoId = (SELECT TOP 1 FondoId FROM dbo.Fondos ORDER BY FondoId)
WHERE  FondoId IS NULL;
GO

-- ---------------------------------------------------------------------
-- 3) Llaves foráneas (relaciones). Solo se crean si todavía no existen.
-- ---------------------------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE parent_object_id = OBJECT_ID(N'dbo.Fotos')
                 AND referenced_object_id = OBJECT_ID(N'dbo.Animales'))
    ALTER TABLE dbo.Fotos
        ADD CONSTRAINT FK_Fotos_Animales_AnimalId
        FOREIGN KEY (AnimalId) REFERENCES dbo.Animales (AnimalId) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE parent_object_id = OBJECT_ID(N'dbo.Solicitudes')
                 AND referenced_object_id = OBJECT_ID(N'dbo.Animales'))
    ALTER TABLE dbo.Solicitudes
        ADD CONSTRAINT FK_Solicitudes_Animales_AnimalId
        FOREIGN KEY (AnimalId) REFERENCES dbo.Animales (AnimalId) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE parent_object_id = OBJECT_ID(N'dbo.Solicitudes')
                 AND referenced_object_id = OBJECT_ID(N'dbo.Usuarios'))
    ALTER TABLE dbo.Solicitudes
        ADD CONSTRAINT FK_Solicitudes_Usuarios_UsuarioId
        FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (UsuarioId) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE parent_object_id = OBJECT_ID(N'dbo.Donaciones')
                 AND referenced_object_id = OBJECT_ID(N'dbo.Usuarios'))
    ALTER TABLE dbo.Donaciones
        ADD CONSTRAINT FK_Donaciones_Usuarios_UsuarioId
        FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (UsuarioId) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE parent_object_id = OBJECT_ID(N'dbo.Donaciones')
                 AND referenced_object_id = OBJECT_ID(N'dbo.Fondos'))
    ALTER TABLE dbo.Donaciones
        ADD CONSTRAINT FK_Donaciones_Fondos_FondoId
        FOREIGN KEY (FondoId) REFERENCES dbo.Fondos (FondoId) ON DELETE SET NULL;
GO

-- Índices sobre las llaves foráneas (consultas más rápidas)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Solicitudes_UsuarioId' AND object_id = OBJECT_ID(N'dbo.Solicitudes'))
    CREATE INDEX IX_Solicitudes_UsuarioId ON dbo.Solicitudes (UsuarioId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donaciones_UsuarioId' AND object_id = OBJECT_ID(N'dbo.Donaciones'))
    CREATE INDEX IX_Donaciones_UsuarioId ON dbo.Donaciones (UsuarioId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donaciones_FondoId' AND object_id = OBJECT_ID(N'dbo.Donaciones'))
    CREATE INDEX IX_Donaciones_FondoId ON dbo.Donaciones (FondoId);
GO

-- ---------------------------------------------------------------------
-- 4) COMPROBACIÓN (estas tres tablas de resultados sirven de captura para el reporte)
-- ---------------------------------------------------------------------

-- 4a) Lista de tablas de la base y cuántas filas tiene cada una
--     (__EFMigrationsHistory la crea Entity Framework solo; es normal que aparezca)
SELECT t.name AS Tabla,
       SUM(CASE WHEN p.index_id IN (0, 1) THEN p.rows ELSE 0 END) AS Filas
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id
GROUP BY t.name
ORDER BY t.name;

-- 4b) Relaciones entre tablas: la tabla "hija" apunta a la tabla "padre"
SELECT fk.name                          AS Relacion,
       OBJECT_NAME(fk.parent_object_id) AS TablaHija,
       cp.name                          AS ColumnaHija,
       OBJECT_NAME(fk.referenced_object_id) AS TablaPadre,
       cr.name                          AS ColumnaPadre,
       fk.delete_referential_action_desc AS AlBorrarElPadre
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns cp ON cp.object_id = fkc.parent_object_id     AND cp.column_id = fkc.parent_column_id
JOIN sys.columns cr ON cr.object_id = fkc.referenced_object_id AND cr.column_id = fkc.referenced_column_id
ORDER BY TablaHija, Relacion;

-- 4c) Columnas de cada tabla (diccionario de datos)
SELECT t.name AS Tabla,
       c.name AS Columna,
       ty.name + CASE WHEN ty.name IN ('nvarchar','varchar') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length / CASE WHEN ty.name = 'nvarchar' THEN 2 ELSE 1 END AS VARCHAR(10)) END + ')'
                          WHEN ty.name = 'decimal' THEN '(' + CAST(c.precision AS VARCHAR(5)) + ',' + CAST(c.scale AS VARCHAR(5)) + ')'
                          ELSE '' END AS Tipo,
       CASE WHEN c.is_nullable = 1 THEN 'Sí' ELSE 'No' END AS AceptaNulos,
       CASE WHEN pk.column_id IS NOT NULL THEN 'PK' ELSE '' END
         + CASE WHEN fkc.parent_column_id IS NOT NULL THEN ' FK' ELSE '' END AS Llave
FROM sys.tables t
JOIN sys.columns c  ON c.object_id = t.object_id
JOIN sys.types ty   ON ty.user_type_id = c.user_type_id
LEFT JOIN (SELECT ic.object_id, ic.column_id
           FROM sys.indexes i
           JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
           WHERE i.is_primary_key = 1) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
LEFT JOIN sys.foreign_key_columns fkc ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
WHERE t.name <> '__EFMigrationsHistory'
ORDER BY t.name, c.column_id;
GO

-- ---------------------------------------------------------------------
-- Para VER las relaciones dibujadas en SSMS (diagrama):
--   Explorador de objetos > AdopcionBd > clic derecho en "Diagramas de base de datos"
--   > "Nuevo diagrama de base de datos" (si pregunta si crear los objetos de soporte: Sí)
--   > selecciona las 6 tablas (Animales, Fotos, Solicitudes, Donaciones, Usuarios, Fondos)
--   > Agregar > Cerrar. Las líneas con llave son las relaciones. Guárdalo como "DiagramaAdopi".
--
-- Si el diagrama marca el error "Cannot execute as the database principal because the
-- principal 'dbo' does not exist", ejecuta esto una vez y vuelve a intentarlo:
--   ALTER AUTHORIZATION ON DATABASE::AdopcionBd TO [sa];
-- ---------------------------------------------------------------------
