-- =====================================================================
-- Kem'il Editorial - Datos de catálogo para la página
-- Agrega géneros, editoriales, autores, bodegas, 14 libros con precio,
-- relación libro-autor e inventario.
--
-- Es REPETIBLE: se puede ejecutar varias veces sin duplicar datos
-- (busca por nombre / ISBN antes de insertar).
-- Ejecutar DESPUÉS de schema.sql y seed.sql.
-- =====================================================================
USE kemilDB;
GO

-- ---------------------------------------------------------------------
-- 1. Géneros
-- ---------------------------------------------------------------------
INSERT INTO genero (nombre, descripcion, estado)
SELECT v.nombre, v.descripcion, 1
FROM (VALUES
    (N'Poesía',     N'Obras líricas y poéticas regionales'),
    (N'Historia',   N'Investigaciones y crónicas históricas'),
    (N'Narrativa',  N'Novela, cuento y relato'),
    (N'Ensayo',     N'Ensayo cultural, social y político'),
    (N'Infantil',   N'Literatura para niñas y niños'),
    (N'Educativo',  N'Textos de apoyo para escuelas y universidades'),
    (N'Gastronomía',N'Cocina y tradiciones culinarias')
) AS v(nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM genero g WHERE g.nombre = v.nombre);
GO

-- ---------------------------------------------------------------------
-- 2. Editoriales
-- ---------------------------------------------------------------------
INSERT INTO editorial (nombre, email, num_telefonico)
SELECT v.nombre, v.email, v.tel
FROM (VALUES
    (N'Kem''il Editorial',            'contacto@kemileditorial.com', '+502 7766-5544'),
    (N'Ediciones del Altiplano',      'info@edicionesaltiplano.gt',  '+502 7761-1020'),
    (N'Casa del Libro Quetzalteca',   'ventas@casadellibro.gt',      '+502 7765-3030'),
    (N'Editorial Voces Nuevas',       'hola@vocesnuevas.gt',         '+502 2230-4455')
) AS v(nombre, email, tel)
WHERE NOT EXISTS (SELECT 1 FROM editorial e WHERE e.nombre = v.nombre);
GO

-- ---------------------------------------------------------------------
-- 3. Bodegas
-- ---------------------------------------------------------------------
INSERT INTO stock (nombre, ubicacion, descripcion)
SELECT v.nombre, v.ubicacion, v.descripcion
FROM (VALUES
    ('Bodega Central Totonicapán', 'Zona 1, Totonicapán',     'Almacén principal de libros impresos'),
    ('Bodega Quetzaltenango',      'Zona 3, Quetzaltenango',  'Almacén de distribución regional')
) AS v(nombre, ubicacion, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM stock s WHERE s.nombre = v.nombre);
GO

-- ---------------------------------------------------------------------
-- 4. Autores (nombres ficticios)
-- ---------------------------------------------------------------------
INSERT INTO autor (nombre, apellido, nacionalidad, estado)
SELECT v.nombre, v.apellido, 'Guatemalteca', 1
FROM (VALUES
    (N'Editorial',  N'Kem''il'),
    (N'Comunitario',N'Autor'),
    (N'Lucía',      N'Tzoc'),
    (N'Mateo',      N'Xicay'),
    (N'Ana',        N'Coyoy'),
    (N'Rodrigo',    N'Batz'),
    (N'Carmen',     N'Ixcoy'),
    (N'Daniel',     N'Sacor'),
    (N'Elena',      N'Ajú'),
    (N'Pablo',      N'Chiroy')
) AS v(nombre, apellido)
WHERE NOT EXISTS (SELECT 1 FROM autor a WHERE a.nombre = v.nombre AND a.apellido = v.apellido);
GO

-- ---------------------------------------------------------------------
-- 5. Libros, relación libro-autor e inventario
--    (portada queda en NULL: la página genera una portada tipográfica.
--     Para usar imagen real: UPDATE libro SET portada = '/portadas/x.jpg' WHERE ISBN = '...')
-- ---------------------------------------------------------------------
DECLARE @Catalogo TABLE (
    isbn        VARCHAR(50),
    nombre      NVARCHAR(200),
    descripcion NVARCHAR(500),
    genero      NVARCHAR(100),
    editorial   NVARCHAR(150),
    autorNombre NVARCHAR(100),
    autorApell  NVARCHAR(100),
    formato     VARCHAR(50),
    precio      DECIMAL(10,2),
    fecha       DATE,
    edicion     VARCHAR(50),
    valoracion  DECIMAL(3,2),
    stockCentral INT,
    stockQuetz   INT
);

INSERT INTO @Catalogo VALUES
('978-99922-0-001-1', N'Antología Poética de Totonicapán', N'Recopilación de poesía contemporánea de la región.',
    N'Poesía', N'Kem''il Editorial', N'Editorial', N'Kem''il', 'Tapa blanda', 125.00, '2023-03-15', 'Primera', 4.60, 50, 20),
('978-99922-0-002-8', N'Historia y Tradición Local', N'Estudio profundo sobre las raíces comunitarias.',
    N'Historia', N'Kem''il Editorial', N'Comunitario', N'Autor', 'Tapa dura', 85.00, '2022-09-01', 'Segunda', 4.40, 30, 15),
('978-99922-0-003-5', N'Voces del Altiplano', N'Cuentos breves sobre la vida en los pueblos del occidente guatemalteco.',
    N'Narrativa', N'Ediciones del Altiplano', N'Lucía', N'Tzoc', 'Tapa blanda', 95.00, '2024-02-10', 'Primera', 4.70, 40, 25),
('978-99922-0-004-2', N'Cenizas de Volcán', N'Novela sobre tres generaciones de una familia junto al volcán.',
    N'Narrativa', N'Casa del Libro Quetzalteca', N'Mateo', N'Xicay', 'Tapa dura', 165.00, '2021-11-20', 'Tercera', 4.80, 18, 10),
('978-99922-0-005-9', N'Cuentos para Soñar en Kaqchikel', N'Cuentos ilustrados para primeros lectores, en español y kaqchikel.',
    N'Infantil', N'Editorial Voces Nuevas', N'Ana', N'Coyoy', 'Tapa dura', 110.00, '2024-05-05', 'Primera', 4.90, 60, 30),
('978-99922-0-006-6', N'El Jaguar y la Luna', N'Álbum infantil con ilustraciones a color sobre una leyenda maya.',
    N'Infantil', N'Kem''il Editorial', N'Ana', N'Coyoy', 'Tapa blanda', 75.00, '2023-08-12', 'Primera', 4.50, 45, 20),
('978-99922-0-007-3', N'Matemática Básica para Secundaria', N'Texto de apoyo con ejercicios resueltos para ciclo básico.',
    N'Educativo', N'Ediciones del Altiplano', N'Rodrigo', N'Batz', 'Tapa blanda', 140.00, '2022-01-30', 'Cuarta', 4.10, 120, 80),
('978-99922-0-008-0', N'Introducción a la Programación', N'Fundamentos de algoritmos y lógica con ejemplos prácticos.',
    N'Educativo', N'Editorial Voces Nuevas', N'Daniel', N'Sacor', 'E-book', 60.00, '2024-07-01', 'Primera', 4.30, 999, 0),
('978-99922-0-009-7', N'Ensayos sobre Memoria y Territorio', N'Reflexiones sobre identidad, tierra y comunidad en Guatemala.',
    N'Ensayo', N'Casa del Libro Quetzalteca', N'Carmen', N'Ixcoy', 'Tapa blanda', 118.00, '2023-10-18', 'Primera', 4.20, 22, 8),
('978-99922-0-010-3', N'Sabores de mi Tierra', N'Recetario tradicional con platillos típicos de Los Altos.',
    N'Gastronomía', N'Kem''il Editorial', N'Elena', N'Ajú', 'Tapa dura', 190.00, '2024-01-25', 'Primera', 4.80, 35, 15),
('978-99922-0-011-0', N'Crónicas de la Ciudad de los Altos', N'Relatos históricos de Quetzaltenango a lo largo de los siglos.',
    N'Historia', N'Casa del Libro Quetzalteca', N'Pablo', N'Chiroy', 'Tapa dura', 155.00, '2020-06-14', 'Segunda', 4.50, 3, 2),
('978-99922-0-012-7', N'Poemas del Río y la Montaña', N'Poesía de autores jóvenes de la región.',
    N'Poesía', N'Editorial Voces Nuevas', N'Lucía', N'Tzoc', 'E-book', 45.00, '2024-09-09', 'Primera', 4.40, 999, 0),
('978-99922-0-013-4', N'Guía de Historia de Guatemala', N'Resumen ilustrado de los grandes periodos históricos del país.',
    N'Educativo', N'Kem''il Editorial', N'Comunitario', N'Autor', 'Tapa blanda', 130.00, '2021-03-03', 'Quinta', 4.00, 0, 0),
('978-99922-0-014-1', N'La Casa de Adobe', N'Novela corta sobre la memoria de una casa familiar.',
    N'Narrativa', N'Ediciones del Altiplano', N'Mateo', N'Xicay', 'Tapa blanda', 99.00, '2023-12-02', 'Primera', 4.60, 28, 12);

-- 5a. Libros nuevos
INSERT INTO libro (id_genero, id_editorial, nombre, descripcion, formato, portada, fecha_lanzamiento, precio, edicion, ISBN, valoracion, estado)
SELECT g.id_genero, e.id_editorial, c.nombre, c.descripcion, c.formato, NULL, c.fecha, c.precio, c.edicion, c.isbn, c.valoracion, 1
FROM @Catalogo c
JOIN genero g    ON g.nombre = c.genero
JOIN editorial e ON e.nombre = c.editorial
WHERE NOT EXISTS (SELECT 1 FROM libro l WHERE l.ISBN = c.isbn);

-- 5b. Completar datos de los libros que ya existían (formato, fecha, edición, valoración)
UPDATE l
SET l.formato = c.formato,
    l.fecha_lanzamiento = COALESCE(l.fecha_lanzamiento, c.fecha),
    l.edicion = COALESCE(l.edicion, c.edicion),
    l.valoracion = COALESCE(l.valoracion, c.valoracion)
FROM libro l
JOIN @Catalogo c ON c.isbn = l.ISBN;

-- 5c. Relación libro-autor
INSERT INTO libro_autor (id_autor, id_libro, descripcion)
SELECT a.id_autor, l.id_libro, 'Autor principal'
FROM @Catalogo c
JOIN libro l ON l.ISBN = c.isbn
JOIN autor a ON a.nombre = c.autorNombre AND a.apellido = c.autorApell
WHERE NOT EXISTS (SELECT 1 FROM libro_autor la WHERE la.id_libro = l.id_libro AND la.id_autor = a.id_autor);

-- 5d. Inventario por bodega (si el libro ya tenía inventario en esa bodega, no se toca)
INSERT INTO inventario_libro (id_stock, id_libro, stock_disponible, stock_minimo, estado)
SELECT s.id_stock, l.id_libro, c.stockCentral, 5, 1
FROM @Catalogo c
JOIN libro l ON l.ISBN = c.isbn
JOIN stock s ON s.nombre = 'Bodega Central Totonicapán'
WHERE NOT EXISTS (SELECT 1 FROM inventario_libro i WHERE i.id_libro = l.id_libro AND i.id_stock = s.id_stock);

INSERT INTO inventario_libro (id_stock, id_libro, stock_disponible, stock_minimo, estado)
SELECT s.id_stock, l.id_libro, c.stockQuetz, 5, 1
FROM @Catalogo c
JOIN libro l ON l.ISBN = c.isbn
JOIN stock s ON s.nombre = 'Bodega Quetzaltenango'
WHERE c.stockQuetz > 0
  AND NOT EXISTS (SELECT 1 FROM inventario_libro i WHERE i.id_libro = l.id_libro AND i.id_stock = s.id_stock);
GO

-- ---------------------------------------------------------------------
-- 6. Verificación rápida
-- ---------------------------------------------------------------------
SELECT l.id_libro, l.nombre, g.nombre AS genero, l.formato, l.precio,
       ISNULL(SUM(i.stock_disponible), 0) AS stock_total
FROM libro l
JOIN genero g ON g.id_genero = l.id_genero
LEFT JOIN inventario_libro i ON i.id_libro = l.id_libro AND i.estado = 1
WHERE l.estado = 1
GROUP BY l.id_libro, l.nombre, g.nombre, l.formato, l.precio
ORDER BY l.id_libro;
GO
