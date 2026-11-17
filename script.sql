-- ============================================================================
-- SCRIPT DE BASE DE DATOS: TamboDB (E-commerce Tambo+)
-- Arquitectura Orientada a Servicios (SOA)
-- Servicios soportados:
--   a) Registro y autenticación (Clientes, Credenciales, Direcciones)
--   b) Catálogo de productos (Categorías, Subcategorías, Marcas, Productos)
--   c) Navegación y búsqueda predictiva
--   d) Detalle de producto e Inventario
--   e) Carrito de compras y Promociones / Cupones
--   f) Checkout y Pago (Delivery/Retiro, Cobertura, Comprobantes, Pasarela)
--   g) Seguimiento de pedidos y Delivery (Motorizados, Historial de estados)
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TamboDB')
BEGIN
    CREATE DATABASE TamboDB COLLATE Modern_Spanish_CI_AS;
END
GO

USE TamboDB;
GO

-- ============================================================================
-- 1. ELIMINACIÓN DE TABLAS EN ORDEN DE DEPENDENCIAS (DROP IF EXISTS)
-- ============================================================================
IF OBJECT_ID('dbo.historial_estado_pedido', 'U') IS NOT NULL DROP TABLE dbo.historial_estado_pedido;
IF OBJECT_ID('dbo.entrega', 'U') IS NOT NULL DROP TABLE dbo.entrega;
IF OBJECT_ID('dbo.pago', 'U') IS NOT NULL DROP TABLE dbo.pago;
IF OBJECT_ID('dbo.detalle_pedido', 'U') IS NOT NULL DROP TABLE dbo.detalle_pedido;
IF OBJECT_ID('dbo.pedido', 'U') IS NOT NULL DROP TABLE dbo.pedido;
IF OBJECT_ID('dbo.detalle_carrito', 'U') IS NOT NULL DROP TABLE dbo.detalle_carrito;
IF OBJECT_ID('dbo.carrito', 'U') IS NOT NULL DROP TABLE dbo.carrito;
IF OBJECT_ID('dbo.cupon_uso', 'U') IS NOT NULL DROP TABLE dbo.cupon_uso;
IF OBJECT_ID('dbo.cupon', 'U') IS NOT NULL DROP TABLE dbo.cupon;
IF OBJECT_ID('dbo.promocion_producto', 'U') IS NOT NULL DROP TABLE dbo.promocion_producto;
IF OBJECT_ID('dbo.promocion', 'U') IS NOT NULL DROP TABLE dbo.promocion;
IF OBJECT_ID('dbo.banner', 'U') IS NOT NULL DROP TABLE dbo.banner;
IF OBJECT_ID('dbo.inventario', 'U') IS NOT NULL DROP TABLE dbo.inventario;
IF OBJECT_ID('dbo.producto', 'U') IS NOT NULL DROP TABLE dbo.producto;
IF OBJECT_ID('dbo.marca', 'U') IS NOT NULL DROP TABLE dbo.marca;
IF OBJECT_ID('dbo.categoria', 'U') IS NOT NULL DROP TABLE dbo.categoria;
IF OBJECT_ID('dbo.direccion', 'U') IS NOT NULL DROP TABLE dbo.direccion;
IF OBJECT_ID('dbo.tienda', 'U') IS NOT NULL DROP TABLE dbo.tienda;
IF OBJECT_ID('dbo.zona_despacho', 'U') IS NOT NULL DROP TABLE dbo.zona_despacho;
IF OBJECT_ID('dbo.cliente', 'U') IS NOT NULL DROP TABLE dbo.cliente;
IF OBJECT_ID('dbo.motorizado', 'U') IS NOT NULL DROP TABLE dbo.motorizado;
IF OBJECT_ID('dbo.estado_pedido', 'U') IS NOT NULL DROP TABLE dbo.estado_pedido;
IF OBJECT_ID('dbo.metodo_pago', 'U') IS NOT NULL DROP TABLE dbo.metodo_pago;
IF OBJECT_ID('dbo.tipo_comprobante', 'U') IS NOT NULL DROP TABLE dbo.tipo_comprobante;
GO

-- ============================================================================
-- 2. TABLAS MAESTRAS Y CATÁLOGOS BASE
-- ============================================================================

-- Catálogo de tipos de comprobante de pago (Requerimiento 15)
CREATE TABLE dbo.tipo_comprobante (
    tipo_comprobante_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    codigo NVARCHAR(20) NOT NULL UNIQUE, -- 'BOLETA', 'FACTURA'
    nombre NVARCHAR(50) NOT NULL,
    requiere_ruc BIT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Catálogo de métodos de pago (Requerimientos 16, 17, 18)
CREATE TABLE dbo.metodo_pago (
    metodo_pago_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    codigo NVARCHAR(50) NOT NULL UNIQUE, -- 'TARJETA_CREDITO_DEBITO', 'PAGOEFECTIVO', 'NIUBIZ_YAPE'
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(255) NULL,
    icono_url NVARCHAR(500) NULL,
    es_online BIT NOT NULL DEFAULT 1,
    borrado BIT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Catálogo de estados de pedido y secuencia de ciclo de vida (Requerimiento 19)
-- Secuencia: Confirmado -> Preparando -> En camino -> Entregado (+ Pendiente / Cancelado)
CREATE TABLE dbo.estado_pedido (
    estado_pedido_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    codigo NVARCHAR(30) NOT NULL UNIQUE, -- 'CONFIRMADO', 'PREPARANDO', 'EN_CAMINO', 'ENTREGADO', 'CANCELADO'
    nombre NVARCHAR(100) NOT NULL,
    orden_secuencia INT NOT NULL,
    descripcion NVARCHAR(255) NULL
);
GO

-- Motorizados y repartidores para entregas (Requerimiento 19)
CREATE TABLE dbo.motorizado (
    motorizado_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombres NVARCHAR(100) NOT NULL,
    apellidos NVARCHAR(100) NOT NULL,
    celular NVARCHAR(20) NOT NULL,
    placa_vehiculo NVARCHAR(20) NOT NULL,
    tipo_vehiculo NVARCHAR(50) NOT NULL DEFAULT 'Moto lineal',
    activo BIT NOT NULL DEFAULT 1,
    borrado BIT NOT NULL DEFAULT 0
);
GO

-- Zonas de despacho para cobertura de delivery (Requerimientos 4, 14)
CREATE TABLE dbo.zona_despacho (
    zona_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    distrito NVARCHAR(100) NOT NULL,
    codigo_postal NVARCHAR(20) NULL,
    costo_delivery DECIMAL(10,2) NOT NULL DEFAULT 5.00,
    tiempo_estimado_min INT NOT NULL DEFAULT 45,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Tiendas físicas Tambo+ (Requerimientos 3, 4, 5, 16)
CREATE TABLE dbo.tienda (
    tienda_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    zona_id INT NULL,
    codigo_tienda NVARCHAR(30) NOT NULL UNIQUE,
    nombre NVARCHAR(150) NOT NULL,
    direccion NVARCHAR(255) NOT NULL,
    distrito NVARCHAR(100) NOT NULL,
    telefono NVARCHAR(20) NULL,
    latitud DECIMAL(9,6) NULL,
    longitud DECIMAL(9,6) NULL,
    horario_atencion NVARCHAR(100) NOT NULL DEFAULT '24 Horas',
    delivery_activo BIT NOT NULL DEFAULT 1,
    retiro_activo BIT NOT NULL DEFAULT 1,
    borrado BIT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_tienda_zona FOREIGN KEY (zona_id) REFERENCES dbo.zona_despacho(zona_id)
);
GO

-- ============================================================================
-- 3. GESTIÓN DE CLIENTES Y AUTENTICACIÓN (Requerimientos 12, 13, 14)
-- ============================================================================

-- Clientes registrados
CREATE TABLE dbo.cliente (
    cliente_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    tipo_documento NVARCHAR(20) NOT NULL, -- 'DNI', 'CE', 'PASAPORTE'
    numero_documento NVARCHAR(20) NOT NULL,
    nombres NVARCHAR(100) NOT NULL,
    apellidos NVARCHAR(100) NOT NULL,
    correo NVARCHAR(150) NOT NULL,
    celular NVARCHAR(20) NOT NULL,
    contrasena NVARCHAR(255) NOT NULL, -- Almacena Hash seguro (BCrypt/Argon2/PBKDF2)
    activo BIT NOT NULL DEFAULT 1,
    borrado BIT NOT NULL DEFAULT 0,
    fecha_registro DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ultimo_acceso DATETIME2 NULL,
    CONSTRAINT UQ_cliente_documento UNIQUE (tipo_documento, numero_documento),
    CONSTRAINT UQ_cliente_correo UNIQUE (correo)
);
GO

-- Direcciones guardadas del cliente (Requerimientos 4, 14)
CREATE TABLE dbo.direccion (
    direccion_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    cliente_id INT NOT NULL,
    zona_id INT NULL,
    alias NVARCHAR(50) NOT NULL DEFAULT 'Mi Dirección', -- 'Casa', 'Trabajo', etc.
    distrito NVARCHAR(100) NOT NULL,
    direccion_texto NVARCHAR(255) NOT NULL,
    numero NVARCHAR(20) NULL,
    departamento_interior NVARCHAR(20) NULL,
    referencia NVARCHAR(255) NULL,
    latitud DECIMAL(9,6) NULL,
    longitud DECIMAL(9,6) NULL,
    principal BIT NOT NULL DEFAULT 0,
    borrado BIT NOT NULL DEFAULT 0,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_direccion_cliente FOREIGN KEY (cliente_id) REFERENCES dbo.cliente(cliente_id),
    CONSTRAINT FK_direccion_zona FOREIGN KEY (zona_id) REFERENCES dbo.zona_despacho(zona_id)
);
GO

-- ============================================================================
-- 4. CATÁLOGO DE PRODUCTOS E INVENTARIO (Requerimientos 1, 2, 6, 7, 9)
-- ============================================================================

-- Categorías jerárquicas (Categorías y Subcategorías en la misma tabla)
CREATE TABLE dbo.categoria (
    categoria_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    categoria_padre_id INT NULL,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(255) NULL,
    icono_url NVARCHAR(500) NULL,
    imagen_url NVARCHAR(500) NULL,
    orden INT NOT NULL DEFAULT 0,
    borrado BIT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_categoria_padre FOREIGN KEY (categoria_padre_id) REFERENCES dbo.categoria(categoria_id)
);
GO

-- Marcas
CREATE TABLE dbo.marca (
    marca_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL UNIQUE,
    borrado BIT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Productos
CREATE TABLE dbo.producto (
    producto_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    categoria_id INT NOT NULL,
    marca_id INT NOT NULL,
    sku NVARCHAR(50) NOT NULL UNIQUE,
    nombre NVARCHAR(200) NOT NULL,
    descripcion NVARCHAR(MAX) NULL,
    imagen_url NVARCHAR(500) NULL,
    precio DECIMAL(10,2) NOT NULL,
    precio_oferta DECIMAL(10,2) NULL,
    unidad_medida NVARCHAR(50) NOT NULL DEFAULT 'Unidad',
    es_destacado BIT NOT NULL DEFAULT 0,
    borrado BIT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_producto_categoria FOREIGN KEY (categoria_id) REFERENCES dbo.categoria(categoria_id),
    CONSTRAINT FK_producto_marca FOREIGN KEY (marca_id) REFERENCES dbo.marca(marca_id)
);
GO

-- Inventario por tienda física (Requerimientos 9, 10, 16)
CREATE TABLE dbo.inventario (
    inventario_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    tienda_id INT NOT NULL,
    producto_id INT NOT NULL,
    stock_disponible INT NOT NULL DEFAULT 0,
    stock_reservado INT NOT NULL DEFAULT 0,
    stock_minimo INT NOT NULL DEFAULT 3,
    ultima_actualizacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_inventario_tienda FOREIGN KEY (tienda_id) REFERENCES dbo.tienda(tienda_id),
    CONSTRAINT FK_inventario_producto FOREIGN KEY (producto_id) REFERENCES dbo.producto(producto_id),
    CONSTRAINT UQ_inventario_tienda_producto UNIQUE (tienda_id, producto_id)
);
GO

-- ============================================================================
-- 5. PROMOCIONES, BANNERS Y CUPONES (Requerimientos 8, 9, 11)
-- ============================================================================

-- Banners publicitarios en Home
CREATE TABLE dbo.banner (
    banner_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    titulo NVARCHAR(150) NOT NULL,
    imagen_url NVARCHAR(500) NOT NULL,
    enlace_destino NVARCHAR(500) NULL,
    posicion INT NOT NULL DEFAULT 1,
    fecha_inicio DATETIME2 NOT NULL,
    fecha_fin DATETIME2 NOT NULL,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Promociones y Tambo Packs
CREATE TABLE dbo.promocion (
    promocion_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    banner_id INT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(500) NULL,
    tipo_promocion NVARCHAR(50) NOT NULL DEFAULT 'TAMBO_PACK', -- 'TAMBO_PACK', 'DESCUENTO_PORCENTAJE', 'PRECIO_ESPECIAL'
    precio_promocional DECIMAL(10,2) NULL,
    porcentaje_descuento DECIMAL(5,2) NULL,
    cantidad_pack INT NOT NULL DEFAULT 1,
    max_por_cliente INT NOT NULL DEFAULT 3, -- Regla de negocio servicio 9
    fecha_inicio DATETIME2 NOT NULL,
    fecha_fin DATETIME2 NOT NULL,
    activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_promocion_banner FOREIGN KEY (banner_id) REFERENCES dbo.banner(banner_id)
);
GO

-- Relación de productos en cada promoción / combo
CREATE TABLE dbo.promocion_producto (
    promocion_id INT NOT NULL,
    producto_id INT NOT NULL,
    cantidad INT NOT NULL DEFAULT 1,
    PRIMARY KEY (promocion_id, producto_id),
    CONSTRAINT FK_promocion_prod_promocion FOREIGN KEY (promocion_id) REFERENCES dbo.promocion(promocion_id),
    CONSTRAINT FK_promocion_prod_producto FOREIGN KEY (producto_id) REFERENCES dbo.producto(producto_id)
);
GO

-- Cupones de descuento (Requerimiento 11)
CREATE TABLE dbo.cupon (
    cupon_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    codigo NVARCHAR(50) NOT NULL UNIQUE,
    descripcion NVARCHAR(200) NULL,
    tipo_descuento NVARCHAR(20) NOT NULL DEFAULT 'PORCENTAJE', -- 'PORCENTAJE', 'MONTO_FIJO'
    valor_descuento DECIMAL(10,2) NOT NULL,
    monto_minimo_compra DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    usos_maximos INT NULL,
    usos_actuales INT NOT NULL DEFAULT 0,
    fecha_inicio DATETIME2 NOT NULL,
    fecha_fin DATETIME2 NOT NULL,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Registro de uso de cupones por cliente
CREATE TABLE dbo.cupon_uso (
    cupon_uso_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    cupon_id INT NOT NULL,
    cliente_id INT NOT NULL,
    fecha_uso DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_cupon_uso_cupon FOREIGN KEY (cupon_id) REFERENCES dbo.cupon(cupon_id),
    CONSTRAINT FK_cupon_uso_cliente FOREIGN KEY (cliente_id) REFERENCES dbo.cliente(cliente_id)
);
GO

-- ============================================================================
-- 6. CARRITO DE COMPRAS (Requerimiento 10)
-- ============================================================================

CREATE TABLE dbo.carrito (
    carrito_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    cliente_id INT NOT NULL,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    fecha_actualizacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_carrito_cliente FOREIGN KEY (cliente_id) REFERENCES dbo.cliente(cliente_id)
);
GO

CREATE TABLE dbo.detalle_carrito (
    detalle_carrito_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    carrito_id INT NOT NULL,
    producto_id INT NOT NULL,
    cantidad INT NOT NULL DEFAULT 1,
    precio_unitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_det_carrito_carrito FOREIGN KEY (carrito_id) REFERENCES dbo.carrito(carrito_id),
    CONSTRAINT FK_det_carrito_producto FOREIGN KEY (producto_id) REFERENCES dbo.producto(producto_id),
    CONSTRAINT UQ_detalle_carrito_producto UNIQUE (carrito_id, producto_id)
);
GO

-- ============================================================================
-- 7. PEDIDOS Y CHECKOUT (Requerimientos 3, 4, 14, 15, 16, 17, 18, 19)
-- ============================================================================

CREATE TABLE dbo.pedido (
    pedido_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    numero_pedido NVARCHAR(30) NOT NULL UNIQUE, -- ej: 'TMB-202611-0001'
    cliente_id INT NOT NULL,
    tienda_id INT NOT NULL,
    tipo_entrega NVARCHAR(30) NOT NULL, -- 'DELIVERY', 'RETIRO_TIENDA'
    direccion_id INT NULL,
    tipo_comprobante_id INT NOT NULL,
    doc_comprobante NVARCHAR(20) NOT NULL, -- DNI del comprador o RUC de la empresa
    razon_social NVARCHAR(150) NULL, -- Obligatorio si tipo_comprobante es FACTURA
    direccion_fiscal NVARCHAR(255) NULL, -- Obligatorio si tipo_comprobante es FACTURA
    subtotal DECIMAL(10,2) NOT NULL,
    descuento DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    costo_envio DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    monto_total DECIMAL(10,2) NOT NULL,
    cupon_id INT NULL,
    estado_pedido_id INT NOT NULL,
    notas NVARCHAR(500) NULL,
    fecha_pedido DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_pedido_cliente FOREIGN KEY (cliente_id) REFERENCES dbo.cliente(cliente_id),
    CONSTRAINT FK_pedido_tienda FOREIGN KEY (tienda_id) REFERENCES dbo.tienda(tienda_id),
    CONSTRAINT FK_pedido_direccion FOREIGN KEY (direccion_id) REFERENCES dbo.direccion(direccion_id),
    CONSTRAINT FK_pedido_tipo_comprobante FOREIGN KEY (tipo_comprobante_id) REFERENCES dbo.tipo_comprobante(tipo_comprobante_id),
    CONSTRAINT FK_pedido_cupon FOREIGN KEY (cupon_id) REFERENCES dbo.cupon(cupon_id),
    CONSTRAINT FK_pedido_estado FOREIGN KEY (estado_pedido_id) REFERENCES dbo.estado_pedido(estado_pedido_id)
);
GO

CREATE TABLE dbo.detalle_pedido (
    detalle_pedido_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pedido_id INT NOT NULL,
    producto_id INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_det_pedido_pedido FOREIGN KEY (pedido_id) REFERENCES dbo.pedido(pedido_id),
    CONSTRAINT FK_det_pedido_producto FOREIGN KEY (producto_id) REFERENCES dbo.producto(producto_id)
);
GO

-- ============================================================================
-- 8. PAGOS Y PASARELA (Requerimientos 16, 17, 18)
-- ============================================================================

CREATE TABLE dbo.pago (
    pago_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pedido_id INT NOT NULL,
    metodo_pago_id INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    estado NVARCHAR(30) NOT NULL DEFAULT 'PENDIENTE', -- 'APROBADO', 'RECHAZADO', 'CANCELADO', 'PENDIENTE'
    codigo_transaccion NVARCHAR(100) NULL,
    mensaje_respuesta NVARCHAR(255) NULL,
    tarjeta_marca NVARCHAR(50) NULL, -- 'VISA', 'MASTERCARD', 'AMEX'
    tarjeta_ultimos_digitos NVARCHAR(4) NULL,
    titular_nombre NVARCHAR(100) NULL,
    titular_apellido NVARCHAR(100) NULL,
    titular_correo NVARCHAR(150) NULL,
    fecha_pago DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_pago_pedido FOREIGN KEY (pedido_id) REFERENCES dbo.pedido(pedido_id),
    CONSTRAINT FK_pago_metodo FOREIGN KEY (metodo_pago_id) REFERENCES dbo.metodo_pago(metodo_pago_id)
);
GO

-- ============================================================================
-- 9. ENTREGA Y SEGUIMIENTO DEL PEDIDO (Requerimiento 19)
-- ============================================================================

CREATE TABLE dbo.entrega (
    entrega_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pedido_id INT NOT NULL UNIQUE,
    motorizado_id INT NULL,
    tiempo_estimado_llegada DATETIME2 NULL,
    fecha_asignacion DATETIME2 NULL,
    fecha_salida DATETIME2 NULL,
    fecha_entrega_real DATETIME2 NULL,
    observaciones NVARCHAR(500) NULL,
    CONSTRAINT FK_entrega_pedido FOREIGN KEY (pedido_id) REFERENCES dbo.pedido(pedido_id),
    CONSTRAINT FK_entrega_motorizado FOREIGN KEY (motorizado_id) REFERENCES dbo.motorizado(motorizado_id)
);
GO

-- Historial cronológico de cambios de estado del pedido (Requerimiento 19)
CREATE TABLE dbo.historial_estado_pedido (
    historial_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pedido_id INT NOT NULL,
    estado_pedido_id INT NOT NULL,
    fecha_cambio DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    comentario NVARCHAR(255) NULL,
    CONSTRAINT FK_hist_pedido FOREIGN KEY (pedido_id) REFERENCES dbo.pedido(pedido_id),
    CONSTRAINT FK_hist_estado FOREIGN KEY (estado_pedido_id) REFERENCES dbo.estado_pedido(estado_pedido_id)
);
GO

-- ============================================================================
-- 10. ÍNDICES DE RENDIMIENTO (Performance Tuning para Servicios SOA)
-- ============================================================================

-- Búsqueda predictiva de productos (Servicio 6)
CREATE NONCLUSTERED INDEX IX_producto_nombre_marca ON dbo.producto (nombre, marca_id)
INCLUDE (precio, imagen_url, borrado, activo);

-- Listado de productos por categoría (Servicio 7)
CREATE NONCLUSTERED INDEX IX_producto_categoria ON dbo.producto (categoria_id, borrado, activo)
INCLUDE (nombre, precio, imagen_url);

-- Stock disponible por tienda y producto (Servicios 9, 10, 16)
CREATE NONCLUSTERED INDEX IX_inventario_tienda_producto ON dbo.inventario (tienda_id, producto_id)
INCLUDE (stock_disponible);

-- Autenticación rápida de cliente por correo (Servicio 12)
CREATE NONCLUSTERED INDEX IX_cliente_correo_activo ON dbo.cliente (correo, activo)
INCLUDE (cliente_id, contrasena, nombres, apellidos);

-- Direcciones activas del cliente (Servicio 14)
CREATE NONCLUSTERED INDEX IX_direccion_cliente ON dbo.direccion (cliente_id, principal, borrado);

-- Pedidos por cliente para mis pedidos (Servicio 19)
CREATE NONCLUSTERED INDEX IX_pedido_cliente ON dbo.pedido (cliente_id, fecha_pedido);
GO

-- ============================================================================
-- 11. DATOS SEMILLA (SEED DATA) DE PRUEBA PARA TAMBO+
-- ============================================================================

-- Tipos de comprobante
INSERT INTO dbo.tipo_comprobante (codigo, nombre, requiere_ruc, activo) VALUES
('BOLETA', 'Boleta de Venta Electrónica', 0, 1),
('FACTURA', 'Factura Electrónica', 1, 1);

-- Métodos de pago
INSERT INTO dbo.metodo_pago (codigo, nombre, descripcion, es_online, borrado, activo) VALUES
('TARJETA', 'Tarjeta de Crédito / Débito (Visa, Mastercard, Amex)', 'Pago seguro vía pasarela de pago', 1, 0, 1),
('PAGOEFECTIVO', 'PagoEfectivo', 'Paga a través de tu banca por internet o agentes con código CIP', 1, 0, 1),
('NIUBIZ_YAPE', 'Niubiz con Yape', 'Paga escaneando código QR o número de celular con Yape', 1, 0, 1);

-- Estados de pedido
INSERT INTO dbo.estado_pedido (codigo, nombre, orden_secuencia, descripcion) VALUES
('CONFIRMADO', 'Confirmado', 1, 'Pedido confirmado y pago procesado'),
('PREPARANDO', 'Preparando', 2, 'La tienda asignada está armando tu pedido'),
('EN_CAMINO', 'En camino', 3, 'El motorizado va en camino hacia tu dirección'),
('ENTREGADO', 'Entregado', 4, 'Pedido entregado satisfactoriamente'),
('CANCELADO', 'Cancelado', 99, 'Pedido cancelado');

-- Zonas de despacho
INSERT INTO dbo.zona_despacho (nombre, distrito, codigo_postal, costo_delivery, tiempo_estimado_min, activo) VALUES
('Miraflores Centro', 'Miraflores', '15074', 4.50, 30, 1),
('San Isidro Financiero', 'San Isidro', '15027', 5.00, 35, 1),
('Barranco Tradicional', 'Barranco', '15063', 4.50, 35, 1),
('Santiago de Surco - Chacarilla', 'Santiago de Surco', '15038', 5.50, 40, 1);

-- Tiendas Tambo+
INSERT INTO dbo.tienda (zona_id, codigo_tienda, nombre, direccion, distrito, telefono, latitud, longitud, horario_atencion, delivery_activo, retiro_activo, borrado, activo) VALUES
(1, 'TMB-MIRA-01', 'Tambo+ Schell', 'Calle Schell 385', 'Miraflores', '01-444-1234', -12.122850, -77.029850, '24 Horas', 1, 1, 0, 1),
(1, 'TMB-MIRA-02', 'Tambo+ Benavides', 'Av. Alfredo Benavides 1530', 'Miraflores', '01-444-5678', -12.128910, -77.018250, '24 Horas', 1, 1, 0, 1),
(2, 'TMB-SANI-01', 'Tambo+ Dos de Mayo', 'Av. Dos de Mayo 890', 'San Isidro', '01-221-9876', -12.094520, -77.043120, '24 Horas', 1, 1, 0, 1),
(4, 'TMB-SURC-01', 'Tambo+ Chacarilla', 'Av. Primavera 650', 'Santiago de Surco', '01-372-4567', -12.110540, -76.985410, '06:00 - 23:00', 1, 1, 0, 1);

-- Motorizados
INSERT INTO dbo.motorizado (nombres, apellidos, celular, placa_vehiculo, tipo_vehiculo, activo, borrado) VALUES
('Carlos', 'Mendoza Vega', '987654321', '8765-4B', 'Moto Honda CB190R', 1, 0),
('Luis', 'Pérez Salazar', '998877665', '4321-9C', 'Moto Yamaha FZ', 1, 0);

-- Categorías (Padres)
INSERT INTO dbo.categoria (categoria_padre_id, nombre, descripcion, orden, borrado, activo) VALUES
(NULL, 'Bebidas y Licores', 'Cervezas, piscos, vinos, gaseosas y energizantes', 1, 0, 1),
(NULL, 'Comidas Rápidas', 'Empanadas, sándwiches, pizzas y combos calientes', 2, 0, 1),
(NULL, 'Snacks y Dulces', 'Papas fritas, galletas, chocolates y golosinas', 3, 0, 1),
(NULL, 'Helados y Postres', 'Helados personales, potes y postres fríos', 4, 0, 1);

-- Subcategorías (Hijas de Bebidas y Licores: ID 1)
INSERT INTO dbo.categoria (categoria_padre_id, nombre, descripcion, orden, borrado, activo) VALUES
(1, 'Cervezas', 'Cervezas nacionales e importadas en lata y botella', 1, 0, 1),
(1, 'Licores y Piscos', 'Pisco, whisky, ron, vodka y tequila', 2, 0, 1),
(1, 'Gaseosas y Aguas', 'Gaseosas de 500ml a 3L y agua mineral', 3, 0, 1),
(1, 'Energizantes', 'Red Bull, Monster y Volt', 4, 0, 1);

-- Subcategorías (Hijas de Comidas Rápidas: ID 2)
INSERT INTO dbo.categoria (categoria_padre_id, nombre, descripcion, orden, borrado, activo) VALUES
(2, 'Empanadas y Enrollados', 'Empanadas de carne, pollo, mixtas y enrollados', 1, 0, 1),
(2, 'Sándwiches y Hamburguesas', 'Hamburguesas, choripán y triples', 2, 0, 1),
(2, 'Pizzas Tambo', 'Porciones y pizzas enteras', 3, 0, 1);

-- Marcas
INSERT INTO dbo.marca (nombre, borrado, activo) VALUES
('Tambo', 0, 1),
('Pilsen Callao', 0, 1),
('Cusqueña', 0, 1),
('Coca Cola', 0, 1),
('Inca Kola', 0, 1),
('Lay''s', 0, 1),
('Monster Energy', 0, 1),
('Donofrio', 0, 1);

-- Productos
INSERT INTO dbo.producto (categoria_id, marca_id, sku, nombre, descripcion, precio, precio_oferta, unidad_medida, es_destacado, borrado, activo) VALUES
(5, 2, 'PROD-PILSEN-LATA-6P', 'Six Pack Cerveza Pilsen Callao 355ml Lata', 'Pack de 6 latas de cerveza Pilsen Callao bien helada.', 26.90, 24.50, 'Pack x6', 1, 0, 1),
(5, 3, 'PROD-CUSQ-GOLD-6P', 'Six Pack Cerveza Cusqueña Dorada 330ml Botella', 'Pack de 6 botellas de cerveza Cusqueña Dorada.', 31.90, 28.90, 'Pack x6', 1, 0, 1),
(7, 4, 'PROD-COCA-COLA-15L', 'Gaseosa Coca Cola Sabor Original 1.5L', 'Botella retornable de Coca Cola sabor original.', 6.50, NULL, 'Botella 1.5L', 0, 0, 1),
(7, 5, 'PROD-INCA-KOLA-15L', 'Gaseosa Inca Kola Sabor Original 1.5L', 'Botella de Inca Kola sabor original 1.5L.', 6.50, NULL, 'Botella 1.5L', 0, 0, 1),
(8, 7, 'PROD-MONSTER-GREEN', 'Bebida Energizante Monster Energy 473ml Lata', 'Energizante con sabor original clásico Monster.', 8.90, 7.90, 'Lata 473ml', 1, 0, 1),
(9, 1, 'PROD-EMP-CARNE-CLÁSICA', 'Empanada Clásica de Carne Tambo', 'Tradicional empanada rellena de carne molida sazonada con pasas y huevo.', 4.90, 4.20, 'Unidad', 1, 0, 1),
(9, 1, 'PROD-EMP-POLLO-POLLERÍA', 'Empanada de Pollo a la Brasa Tambo', 'Empanada rellena de jugoso pollo a la brasa deshilachado.', 4.90, 4.20, 'Unidad', 1, 0, 1),
(10, 1, 'PROD-HAMB-EXTREMA', 'Hamburguesa Extrema Doble Carne con Queso', 'Hamburguesa doble carne con queso derretido en pan artesanal.', 9.90, 8.50, 'Unidad', 1, 0, 1),
(4, 8, 'PROD-HELADO-SUBLIME', 'Helado Peziduri Sublime 900ml', 'Helado de crema sabor vainilla con trozos de chocolate Sublime.', 19.90, 16.90, 'Pote 900ml', 1, 0, 1);

-- Inventario en tiendas (Tienda 1: Schell, Tienda 2: Benavides)
INSERT INTO dbo.inventario (tienda_id, producto_id, stock_disponible, stock_reservado, stock_minimo) VALUES
(1, 1, 45, 0, 10), -- Pilsen en Schell
(1, 2, 30, 0, 5),  -- Cusqueña en Schell
(1, 3, 50, 0, 10), -- Coca Cola en Schell
(1, 4, 60, 0, 10), -- Inca Kola en Schell
(1, 5, 25, 0, 5),  -- Monster en Schell
(1, 6, 20, 0, 5),  -- Empanada Carne en Schell
(1, 7, 18, 0, 5),  -- Empanada Pollo en Schell
(1, 8, 15, 0, 3),  -- Hamburguesa en Schell
(1, 9, 12, 0, 2),  -- Helado en Schell
(2, 1, 20, 0, 5),  -- Pilsen en Benavides
(2, 6, 10, 0, 3);  -- Empanada en Benavides

-- Banners y Promociones ("Tambo Packs") (Requerimiento 8)
INSERT INTO dbo.banner (titulo, imagen_url, enlace_destino, posicion, fecha_inicio, fecha_fin, activo) VALUES
('Tambo Pack Fin de Semana', 'https://tambo.pe/banners/tambo-pack-pilsen.jpg', '/promociones/pack-chelero', 1, '2026-01-01', '2026-12-31', 1),
('Bajón Tambo: Empanada + Gaseosa', 'https://tambo.pe/banners/bajon-empanada.jpg', '/promociones/bajon-tambo', 2, '2026-01-01', '2026-12-31', 1);

INSERT INTO dbo.promocion (banner_id, nombre, descripcion, tipo_promocion, precio_promocional, porcentaje_descuento, cantidad_pack, max_por_cliente, fecha_inicio, fecha_fin, activo) VALUES
(1, 'Pack Chelero: 2 Six Pack Pilsen + Hielo Gratis', 'Lleva 2 Six Packs de Pilsen con descuento especial y bolsa de hielo.', 'TAMBO_PACK', 46.90, NULL, 2, 3, '2026-01-01', '2026-12-31', 1),
(2, 'Combo Bajón: Empanada de Carne + Coca Cola 1.5L', 'El clásico bajón de medianoche al mejor precio.', 'TAMBO_PACK', 9.90, NULL, 1, 5, '2026-01-01', '2026-12-31', 1);

INSERT INTO dbo.promocion_producto (promocion_id, producto_id, cantidad) VALUES
(1, 1, 2), -- 2 six packs de Pilsen
(2, 6, 1), -- 1 empanada de carne
(2, 3, 1); -- 1 Coca Cola 1.5L

-- Cupones de descuento (Requerimiento 11)
INSERT INTO dbo.cupon (codigo, descripcion, tipo_descuento, valor_descuento, monto_minimo_compra, usos_maximos, usos_actuales, fecha_inicio, fecha_fin, activo) VALUES
('TAMBO10', '10% de descuento en tu primera compra en la app', 'PORCENTAJE', 10.00, 30.00, 500, 0, '2026-01-01', '2026-12-31', 1),
('ENVIOFREE', 'Envío gratis por compras mayores a S/ 50.00', 'MONTO_FIJO', 5.00, 50.00, 1000, 0, '2026-01-01', '2026-12-31', 1);

-- Cliente de prueba (Contraseña hash simulada para 'Password123!') (Requerimientos 12, 13)
INSERT INTO dbo.cliente (tipo_documento, numero_documento, nombres, apellidos, correo, celular, contrasena, activo, borrado) VALUES
('DNI', '72345678', 'Juan', 'Rodríguez', 'juan.rodriguez@email.com', '987654321', '$2a$12$e8YgSskFqO5Vvj12c0lFz.5W8rI4bY9YJt6aM2QWq7r1K4T8hWz2a', 1, 0);

-- Dirección de prueba (Requerimientos 4, 14)
INSERT INTO dbo.direccion (cliente_id, zona_id, alias, distrito, direccion_texto, numero, referencia, latitud, longitud, principal, borrado) VALUES
(1, 1, 'Casa Miraflores', 'Miraflores', 'Av. José Pardo', '450', 'Frente al parque Kennedy', -12.121500, -77.031200, 1, 0);

PRINT 'Base de datos TamboDB y datos semilla creados exitosamente.';
GO
