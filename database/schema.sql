-- =====================================================================
-- Creación e inicialización de la base de datos Kem'il Editorial
-- =====================================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'kemilDB')
BEGIN
    CREATE DATABASE kemilDB;
END
GO

USE kemilDB;
GO

-- =====================================================================
-- 1. Tablas Independientes (Sin llaves foráneas previas)
-- =====================================================================

CREATE TABLE rol (
    id_rol INT IDENTITY(1,1) PRIMARY KEY,
    nombre_rol VARCHAR(100) NOT NULL,
    permisos VARCHAR(MAX) NULL
);
GO

CREATE TABLE autor (
    id_autor INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    nacionalidad VARCHAR(100) NULL,
    estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE editorial (
    id_editorial INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    email VARCHAR(150) NULL,
    num_telefonico VARCHAR(50) NULL
);
GO

CREATE TABLE genero (
    id_genero INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT NULL,
    estado BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE stock (
    id_stock INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    ubicacion TEXT NULL,
    descripcion VARCHAR(255) NULL
);
GO

-- =====================================================================
-- 2. Tablas dependientes de Nivel 1 (usuarios, libro)
-- =====================================================================

CREATE TABLE usuarios (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    id_rol INT NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    correo VARCHAR(150) NOT NULL UNIQUE,
    contraseña VARCHAR(255) NOT NULL,
    fecha_registro DATE DEFAULT GETDATE(),
    estado BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_usuarios_rol FOREIGN KEY (id_rol) REFERENCES rol(id_rol)
);
GO

CREATE TABLE libro (
    id_libro INT IDENTITY(1,1) PRIMARY KEY,
    id_genero INT NOT NULL,
    id_editorial INT NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    descripcion TEXT NULL,
    formato VARCHAR(50) NULL,
    portada VARCHAR(255) NULL,
    fecha_lanzamiento DATE NULL,
    precio DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    edicion VARCHAR(50) NULL,
    ISBN VARCHAR(50) NULL,
    valoracion DECIMAL(3,2) NULL,
    estado BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_libro_genero FOREIGN KEY (id_genero) REFERENCES genero(id_genero),
    CONSTRAINT FK_libro_editorial FOREIGN KEY (id_editorial) REFERENCES editorial(id_editorial)
);
GO

-- =====================================================================
-- 3. Tablas dependientes de Nivel 2 (Relaciones y Transacciones)
-- =====================================================================

CREATE TABLE factura (
    id_factura INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    fecha_emision DATE DEFAULT GETDATE(),
    subtotal_base DECIMAL(10,2) NOT NULL,
    descuento_aplicado VARCHAR(100) NULL,
    impuestos DECIMAL(10,2) NOT NULL,
    total_final DECIMAL(10,2) NOT NULL,
    estado VARCHAR(50) DEFAULT 'Emitida',
    descuento_volumen_pct DECIMAL(5,2) DEFAULT 0.00,
    descuento_cliente_pct DECIMAL(5,2) DEFAULT 0.00,
    monto_descuento DECIMAL(10,2) DEFAULT 0.00,
    CONSTRAINT FK_factura_usuarios FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
GO

CREATE TABLE detalle_factura (
    id_detalle INT IDENTITY(1,1) PRIMARY KEY,
    id_factura INT NOT NULL,
    id_libro INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    subtotal_linea DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_detalle_factura FOREIGN KEY (id_factura) REFERENCES factura(id_factura),
    CONSTRAINT FK_detalle_libro FOREIGN KEY (id_libro) REFERENCES libro(id_libro)
);
GO

CREATE TABLE libro_autor (
    id_libro_autor INT IDENTITY(1,1) PRIMARY KEY,
    id_autor INT NOT NULL,
    id_libro INT NOT NULL,
    descripcion VARCHAR(255) NULL,
    CONSTRAINT FK_libroautor_autor FOREIGN KEY (id_autor) REFERENCES autor(id_autor),
    CONSTRAINT FK_libroautor_libro FOREIGN KEY (id_libro) REFERENCES libro(id_libro)
);
GO

CREATE TABLE inventario_libro (
    id_inventario INT IDENTITY(1,1) PRIMARY KEY,
    id_stock INT NOT NULL,
    id_libro INT NOT NULL,
    stock_disponible INT NOT NULL DEFAULT 0,
    stock_minimo INT NOT NULL DEFAULT 5,
    estado BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_inventario_stock FOREIGN KEY (id_stock) REFERENCES stock(id_stock),
    CONSTRAINT FK_inventario_libro FOREIGN KEY (id_libro) REFERENCES libro(id_libro)
);
GO

CREATE TABLE cotizaciones (
    id_cotizacion INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_asesor INT NULL, -- Puede ser un usuario con rol de asesor
    titulo_obra VARCHAR(200) NOT NULL,
    cantidad_estimada INT NOT NULL,
    acabado_portada VARCHAR(100) NULL,
    tipo_papel VARCHAR(100) NULL,
    fecha_requerida_entrega DATE NULL,
    fecha_respuesta DATE NULL,
    fecha_solicitud DATE DEFAULT GETDATE(),
    monto_cotizado DECIMAL(10,2) NULL,
    observaciones_asesor TEXT NULL,
    comentarios_cliente TEXT NULL,
    estado VARCHAR(50) DEFAULT 'Pendiente',
    CONSTRAINT FK_cotizaciones_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
GO

CREATE TABLE perfiles_institucional (
    id_perfil INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    nombre_institucion VARCHAR(150) NOT NULL,
    nit VARCHAR(50) NOT NULL,
    tipo_institucion VARCHAR(100) NULL,
    persona_contacto VARCHAR(150) NULL,
    telefono_contacto VARCHAR(50) NULL,
    direccion_fiscal TEXT NULL,
    CONSTRAINT FK_perfil_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
GO

CREATE TABLE carrito (
    id_carrito INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_carrito_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
GO

CREATE TABLE detalle_carrito (
    id_detalle_carrito INT IDENTITY(1,1) PRIMARY KEY,
    id_carrito INT NOT NULL,
    id_libro INT NOT NULL,
    cantidad INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_detallecarrito_carrito FOREIGN KEY (id_carrito) REFERENCES carrito(id_carrito),
    CONSTRAINT FK_detallecarrito_libro FOREIGN KEY (id_libro) REFERENCES libro(id_libro)
);
GO