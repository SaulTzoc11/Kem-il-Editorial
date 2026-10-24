USE kemilDB;
GO

-- =====================================================================
-- Inserción de Datos Iniciales (Seed Data)
-- =====================================================================

-- 1. Roles del sistema
INSERT INTO rol (nombre_rol, permisos) VALUES 
('Administrador', 'ALL'),
('Cliente', 'READ,CREATE_COTIZACION,BUY'),
('Asesor', 'READ,MANAGE_COTIZACION');
GO

-- 2. Géneros literarios
INSERT INTO genero (nombre, descripcion, estado) VALUES 
('Poesía', 'Obras líricas y poéticas regionales', 1),
('Historia', 'Investigaciones y crónicas históricas', 1);
GO

-- 3. Editoriales
INSERT INTO editorial (nombre, email, num_telefonico) VALUES 
('Kem''il Editorial', 'contacto@kemileditorial.com', '+502 7766-5544');
GO

-- 4. Autores
INSERT INTO autor (nombre, apellido, nacionalidad, estado) VALUES 
('Editorial', 'Kem''il', 'Guatemalteca', 1),
('Comunitario', 'Autor', 'Guatemalteca', 1);
GO

-- 5. Ubicaciones de Stock / Bodega
INSERT INTO stock (nombre, ubicacion, descripcion) VALUES 
('Bodega Central Totonicapán', 'Zona 1, Totonicapán', 'Almacén principal de libros impresos');
GO

-- 6. Usuarios de prueba (Nota: La contraseña en producción debe ir hasheada)
INSERT INTO usuarios (id_rol, nombre, correo, contraseña, estado) VALUES 
(1, 'Saúl De León', 'saul@kemileditorial.com', 'hashed_password_saul', 1),
(2, 'Juan Pérez', 'juan.perez@example.com', 'hashed_password_juan', 1);
GO

-- 7. Libros
INSERT INTO libro (id_genero, id_editorial, nombre, descripcion, formato, precio, ISBN, estado) VALUES 
(1, 1, 'Antología Poética de Totonicapán', 'Recopilación de poesía contemporánea de la región.', 'Pasta Blanda', 125.00, '978-99922-0-001-1', 1),
(2, 1, 'Historia y Tradición Local', 'Estudio profundo sobre las raíces comunitarias.', 'Pasta Dura', 85.00, '978-99922-0-002-8', 1);
GO

-- 8. Inventario inicial de los libros
INSERT INTO inventario_libro (id_stock, id_libro, stock_disponible, stock_minimo, estado) VALUES 
(1, 1, 50, 10, 1),
(1, 2, 30, 5, 1);
GO