USE kemilDB;
GO

-- 1. Roles que faltan (Administrador, Cliente y Asesor ya vienen del seed)
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre_rol = 'Mayorista')
    INSERT INTO rol (nombre_rol, permisos) VALUES ('Mayorista', 'READ,BUY,BULK_DISCOUNT');

IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre_rol = 'Institucional')
    INSERT INTO rol (nombre_rol, permisos) VALUES ('Institucional', 'READ,BUY,BULK_DISCOUNT,CREATE_COTIZACION');
GO

-- 2. Un usuario de prueba por rol. Contraseña de todos: Kemil2026!
-- (hashes BCrypt, nunca se guarda la contraseña en texto plano)
IF NOT EXISTS (SELECT 1 FROM usuarios WHERE correo = 'admin@kemileditorial.com')
    INSERT INTO usuarios (id_rol, nombre, correo, password_hash, fecha_registro, estado)
    VALUES ((SELECT id_rol FROM rol WHERE nombre_rol = 'Administrador'),
            'Admin Kem''il', 'admin@kemileditorial.com',
            '$2b$12$5RPFo1xAePgDwDaTsXGLXu8G.3XSkvhfNTXeJShEchpzqBnV/nyLS',
            CAST(GETDATE() AS DATE), 1);

IF NOT EXISTS (SELECT 1 FROM usuarios WHERE correo = 'asesor@kemileditorial.com')
    INSERT INTO usuarios (id_rol, nombre, correo, password_hash, fecha_registro, estado)
    VALUES ((SELECT id_rol FROM rol WHERE nombre_rol = 'Asesor'),
            'Asesor Kem''il', 'asesor@kemileditorial.com',
            '$2b$12$yCRG1pPOvzxbvDyAbshsf.KDelD1CvOKRqvkCNCKWJCgYbTvR6WBG',
            CAST(GETDATE() AS DATE), 1);

IF NOT EXISTS (SELECT 1 FROM usuarios WHERE correo = 'cliente@kemileditorial.com')
    INSERT INTO usuarios (id_rol, nombre, correo, password_hash, fecha_registro, estado)
    VALUES ((SELECT id_rol FROM rol WHERE nombre_rol = 'Cliente'),
            'Cliente de Prueba', 'cliente@kemileditorial.com',
            '$2b$12$7pFipY98D4RhcOjeOXeSZuULoh9jUHdSNgUbygdx.tjzP4swqear.',
            CAST(GETDATE() AS DATE), 1);

IF NOT EXISTS (SELECT 1 FROM usuarios WHERE correo = 'mayorista@kemileditorial.com')
    INSERT INTO usuarios (id_rol, nombre, correo, password_hash, fecha_registro, estado)
    VALUES ((SELECT id_rol FROM rol WHERE nombre_rol = 'Mayorista'),
            'Libreria Mayorista (prueba)', 'mayorista@kemileditorial.com',
            '$2b$12$cOTmNLqGyYm0HWtKkRRXj.k3YM5gAGqs4pX5CGYMMgpoosAmF2.q.',
            CAST(GETDATE() AS DATE), 1);

IF NOT EXISTS (SELECT 1 FROM usuarios WHERE correo = 'institucional@kemileditorial.com')
    INSERT INTO usuarios (id_rol, nombre, correo, password_hash, fecha_registro, estado)
    VALUES ((SELECT id_rol FROM rol WHERE nombre_rol = 'Institucional'),
            'Colegio Institucional (prueba)', 'institucional@kemileditorial.com',
            '$2b$12$LicQUsWPKrE7ER2yDMGNH.z5lKHa01ti8B/EWbEHEatLHAm4utS/G',
            CAST(GETDATE() AS DATE), 1);
GO

-- 3. Verificación
SELECT u.id_usuario, u.nombre, u.correo, r.nombre_rol, u.estado
FROM usuarios u INNER JOIN rol r ON r.id_rol = u.id_rol
ORDER BY u.id_usuario;
GO