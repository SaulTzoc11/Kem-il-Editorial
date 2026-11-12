-- Generado por frontend/scripts/generar-portadas.mjs: asigna la imagen de portada a cada libro por ISBN.
-- Las imágenes viven en frontend/public/portadas/ y se sirven como /portadas/<archivo>.svg
USE kemilDB;
GO

UPDATE libro SET portada = '/portadas/978-99922-0-001-1.svg' WHERE ISBN = '978-99922-0-001-1';
UPDATE libro SET portada = '/portadas/978-99922-0-002-8.svg' WHERE ISBN = '978-99922-0-002-8';
UPDATE libro SET portada = '/portadas/978-99922-0-003-5.svg' WHERE ISBN = '978-99922-0-003-5';
UPDATE libro SET portada = '/portadas/978-99922-0-004-2.svg' WHERE ISBN = '978-99922-0-004-2';
UPDATE libro SET portada = '/portadas/978-99922-0-005-9.svg' WHERE ISBN = '978-99922-0-005-9';
UPDATE libro SET portada = '/portadas/978-99922-0-006-6.svg' WHERE ISBN = '978-99922-0-006-6';
UPDATE libro SET portada = '/portadas/978-99922-0-007-3.svg' WHERE ISBN = '978-99922-0-007-3';
UPDATE libro SET portada = '/portadas/978-99922-0-008-0.svg' WHERE ISBN = '978-99922-0-008-0';
UPDATE libro SET portada = '/portadas/978-99922-0-009-7.svg' WHERE ISBN = '978-99922-0-009-7';
UPDATE libro SET portada = '/portadas/978-99922-0-010-3.svg' WHERE ISBN = '978-99922-0-010-3';
UPDATE libro SET portada = '/portadas/978-99922-0-011-0.svg' WHERE ISBN = '978-99922-0-011-0';
UPDATE libro SET portada = '/portadas/978-99922-0-012-7.svg' WHERE ISBN = '978-99922-0-012-7';
UPDATE libro SET portada = '/portadas/978-99922-0-013-4.svg' WHERE ISBN = '978-99922-0-013-4';
UPDATE libro SET portada = '/portadas/978-99922-0-014-1.svg' WHERE ISBN = '978-99922-0-014-1';
GO
