-- =====================================================================
-- Kem'il Editorial - Limpieza de datos de prueba antes de la entrega
-- Borra órdenes (factura + detalle) y cotizaciones de prueba.
-- NO toca libros, usuarios, roles ni inventario.
--
-- SEGURO POR DEFECTO: con @Confirmar = 0 muestra qué borraría y deshace todo.
-- Para borrar de verdad, cambia @Confirmar a 1 y vuelve a ejecutar.
-- =====================================================================
USE kemilDB;
GO

DECLARE @Confirmar BIT = 0;   -- 0 = solo mostrar (ROLLBACK) | 1 = borrar de verdad (COMMIT)

BEGIN TRANSACTION;

PRINT 'Antes de borrar:';
SELECT (SELECT COUNT(*) FROM factura)         AS facturas,
       (SELECT COUNT(*) FROM detalle_factura) AS detalles,
       (SELECT COUNT(*) FROM cotizaciones)    AS cotizaciones;

-- Lo que se va a borrar
SELECT f.id_factura, u.correo, f.fecha_emision, f.total_final, f.estado
FROM factura f JOIN usuarios u ON u.id_usuario = f.id_usuario;

SELECT c.id_cotizacion, u.correo, c.titulo_obra, c.cantidad_estimada, c.estado
FROM cotizaciones c JOIN usuarios u ON u.id_usuario = c.id_usuario;

-- Primero los hijos, luego los padres (llaves foráneas)
DELETE FROM detalle_factura;
DELETE FROM factura;
DELETE FROM cotizaciones;

-- Reinicia los contadores para que la próxima orden sea la #1
DBCC CHECKIDENT ('factura', RESEED, 0);
DBCC CHECKIDENT ('detalle_factura', RESEED, 0);
DBCC CHECKIDENT ('cotizaciones', RESEED, 0);

PRINT 'Después de borrar:';
SELECT (SELECT COUNT(*) FROM factura)         AS facturas,
       (SELECT COUNT(*) FROM detalle_factura) AS detalles,
       (SELECT COUNT(*) FROM cotizaciones)    AS cotizaciones;

IF @Confirmar = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT 'Datos de prueba borrados.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'Simulación: no se borró nada. Cambia @Confirmar a 1 para borrar.';
END
GO

-- Si hiciste compras de prueba, el stock quedó descontado. Para dejarlo como al inicio
-- (50 unidades por libro físico en la Bodega Central), descomenta y ejecuta:
-- UPDATE inventario_libro SET stock_disponible = 50
-- WHERE id_stock = (SELECT id_stock FROM stock WHERE nombre = 'Bodega Central Totonicapán')
--   AND id_libro IN (SELECT id_libro FROM libro WHERE formato <> 'E-book');
-- GO
