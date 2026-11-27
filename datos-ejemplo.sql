-- Datos de ejemplo para que la página de Animales muestre algo.
-- Ejecutar en SSMS (o en el Explorador de objetos de SQL Server en Visual Studio)
-- sobre la base de datos AdopcionBd. Solo inserta si la tabla Animales está vacía.
USE AdopcionBd;
GO

IF NOT EXISTS (SELECT 1 FROM Animales)
BEGIN
    DECLARE @id INT;

    -- Max (perro). Especie: Gato=1, Perro=2 | Sexo: Macho=1, Hembra=2 | Tamano: Pequeno=1, Mediano=2, Grande=3 | Estado: Disponible=1
    INSERT INTO Animales (Nombre, Especie, Raza, EdadMeses, Sexo, Tamano, Color, Descripcion, Esterilizado, Vacunado, Ciudad, ContactoPublicador, Estado, FechaPublicacion)
    VALUES (N'Max', 2, NULL, 10, 1, 2, NULL, N'Un compañero alegre, listo para explorar.', 0, 1, N'Ciudad', NULL, 1, SYSUTCDATETIME());
    SET @id = SCOPE_IDENTITY();
    INSERT INTO Fotos (AnimalId, Url, EsPrincipal) VALUES (@id, N'/imagenes/max.jpg', 1);

    -- Luna (gata)
    INSERT INTO Animales (Nombre, Especie, Raza, EdadMeses, Sexo, Tamano, Color, Descripcion, Esterilizado, Vacunado, Ciudad, ContactoPublicador, Estado, FechaPublicacion)
    VALUES (N'Luna', 1, NULL, 36, 2, 1, NULL, N'Una gatita curiosa que disfruta los lugares tranquilos.', 1, 1, N'Ciudad', NULL, 1, SYSUTCDATETIME());
    SET @id = SCOPE_IDENTITY();
    INSERT INTO Fotos (AnimalId, Url, EsPrincipal) VALUES (@id, N'/imagenes/luna.jpg', 1);

    -- Sol (perra)
    INSERT INTO Animales (Nombre, Especie, Raza, EdadMeses, Sexo, Tamano, Color, Descripcion, Esterilizado, Vacunado, Ciudad, ContactoPublicador, Estado, FechaPublicacion)
    VALUES (N'Sol', 2, NULL, 8, 2, 2, NULL, N'Una amiga llena de energía y alegría.', 0, 1, N'Ciudad', NULL, 1, SYSUTCDATETIME());
    SET @id = SCOPE_IDENTITY();
    INSERT INTO Fotos (AnimalId, Url, EsPrincipal) VALUES (@id, N'/imagenes/sol.jpg', 1);
END
GO

SELECT a.AnimalId, a.Nombre, a.Especie, f.Url FROM Animales a LEFT JOIN Fotos f ON f.AnimalId = a.AnimalId;
