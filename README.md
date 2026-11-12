# Sistema Supermercado — Inventory

Microservicio de **inventario**: mantiene el stock de cada producto, el historial de movimientos y el
**historial de precios**. Recibe los movimientos de forma **asíncrona desde Kafka** (publicados por Order) y también por HTTP.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Seq, Gateway, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- Stock actual y último precio de cada producto (`PRODUCTO_STOCK`)
- Historial de entradas y salidas (`MOVIMIENTO_STOCK`)
- Historial de cambios de precio (`HISTORIAL_PRECIO`)
- Consumir los eventos de compras y ventas para actualizar el stock

**Base de datos:** `DB_Inventory` — tablas `PRODUCTO_STOCK`, `MOVIMIENTO_STOCK`, `HISTORIAL_PRECIO`.

## Estructura

```
Inventory/       API (controllers, Program.cs, appsettings) y Observabilidad/ (Correlation Id, manejo global de errores)
Domain/          Entidades, DTOs, DbContext, Migrations y Migrations/Scripts (stored procedures .sql)
Infrastructure/  Repositorio, servicios, validadores, Security (usuario actual), Handler del token y Kafka (consumer)
Utilities/       ApiResponse, mensajes y paginación
```

## Endpoints

Vía Gateway: `http://localhost:8000/inventory/...` → aquí `/api/...` · **Todos requieren token.**

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/MovimientoStock` | Registra un movimiento manual (valida en Admin que el producto exista). Stock insuficiente → **400** |
| GET | `/api/ProductoStock` | Stock y precios de todos los productos (lo usa Admin para el asistente de IA) |
| GET | `/api/ProductoStock/{idProducto}` | Stock y precios actuales. **404** si el producto aún no tiene registro de stock |
| GET | `/api/ProductoStock/{idProducto}/historial-precios` | Cambios de precio, del más reciente al más antiguo |

```json
// POST /api/MovimientoStock
{ "idProducto": 5, "tipo": "ENTRADA", "cantidad": 10, "referencia": "AJUSTE", "precioCompra": 1.20, "precioVenta": 1.80 }

// GET /api/ProductoStock/5  (Order lo usa para validar stock antes de vender)
{ "isSuccess": true, "message": "Consulta exitosa.", "data": { "idProducto": 5, "stock": 40, "precioCompra": 1.20, "precioVenta": 1.80 } }

// GET /api/ProductoStock/5/historial-precios
{ "isSuccess": true, "data": [ { "idHistorialPrecio": 3, "idProducto": 5, "precioCompraAnterior": 1.10, "precioCompraNuevo": 1.20,
    "precioVentaAnterior": 1.70, "precioVentaNuevo": 1.80, "referencia": "COMPRA", "idUsuarioCreacion": 1, "fechaCreacion": "2026-11-10T14:40:00Z" } ] }
```

Swagger: http://localhost:8003/swagger (Docker) · https://localhost:7091/swagger (local)

## Auditoría

| Tabla | Columnas de auditoría |
|---|---|
| `PRODUCTO_STOCK` | `ID_USUARIO_CREACION`, `FECHA_CREACION`, `ID_USUARIO_MODIFICACION`, `FECHA_MODIFICACION` (se actualizan en cada movimiento) |
| `MOVIMIENTO_STOCK` | `ID_USUARIO_CREACION`, `FECHA_CREACION` (es inmutable: solo se inserta) |
| `HISTORIAL_PRECIO` | `ID_USUARIO_CREACION`, `FECHA_CREACION` (solo se inserta) |

- **El usuario nunca se toma del body.** En el endpoint HTTP sale del token (`ICurrentUserService`, claim `NameIdentifier`);
  desde Kafka es el `IdUsuario` que envía Order (el usuario que hizo la compra/venta).
- **Las fechas las pone SQL Server en UTC** (`SYSUTCDATETIME()`, columnas `datetime2`).
- No hay FK al usuario: el usuario vive en `DB_Auth`, otra base.

## Historial de precios

`PA_REGISTRAR_MOVIMIENTO_STOCK` inserta un registro en `HISTORIAL_PRECIO`, **en la misma transacción** que el movimiento,
cuando una **ENTRADA** trae un precio de compra o de venta distinto al actual (incluida la primera vez que el producto
obtiene precio). Guarda el precio anterior y el nuevo, la referencia (`COMPRA`, `AJUSTE`...), el usuario y la fecha.
Las SALIDAS nunca cambian precios. La tabla es solo de inserción: sirve como respaldo y para auditar cambios de precio.

## Integraciones

### Kafka (asíncrono) — consumer
`MovimientoStockConsumer` (un `BackgroundService`) escucha el tópico **`movimientos-stock`** que publica **Order**:

```json
{ "IdProducto": 5, "Tipo": "SALIDA", "Cantidad": 2, "Referencia": "VENTA", "PrecioCompra": 0, "PrecioVenta": 1.80, "IdUsuario": 1 }
```
Header del mensaje: `X-Correlation-Id` (UTF-8). Clave del mensaje: el `IdProducto`.

| Origen | `Tipo` | `Referencia` |
|---|---|---|
| Compra | `ENTRADA` | `COMPRA` |
| Venta | `SALIDA` | `VENTA` |

- **Grupo de consumidores:** `inventory-service`. Si levantas varias instancias, Kafka reparte las particiones entre ellas.
- **Confirmación manual del offset** y manejo de errores según su tipo:

| Situación | Nivel de log | Qué hace |
|---|---|---|
| Movimiento registrado | Information | Confirma el mensaje |
| **Stock insuficiente** (SQL `50001`) u otro error de negocio del SP (`50001`–`50999`) | Warning | Confirma y **descarta** (reintentar nunca funcionaría y bloquearía la partición) |
| JSON inválido o no pasa la validación | Warning | Confirma y descarta |
| Fallo técnico (BD caída, timeout...) | Error | **No confirma**: vuelve al mismo mensaje y reintenta cada 5 s |

- No vuelve a validar el producto contra Admin: Order ya lo validó antes de publicar, y el consumer
  no tiene el token del usuario para llamar al Gateway.

### HTTP (síncrono) — a través del Gateway
El endpoint `POST /api/MovimientoStock` consulta a **Admin** (`/admin/Producto/{id}`) reenviando el token del usuario
y el `X-Correlation-Id`.

## Logs (Serilog + Seq)

- Logs estructurados con **Serilog** → consola y **Seq** (Docker: http://localhost:5341; dentro de Docker `http://seq:80`).
  Todos los eventos llevan la propiedad `Servicio = Inventory`.
- **Niveles:** `Information` = todo salió bien · `Warning` = se incumplió una regla de negocio (stock insuficiente,
  validación, producto inexistente; también respuestas 4xx) · `Error` = fallo técnico (excepción, respuestas 5xx).
- **Correlation Id:** cada petición lleva el header `X-Correlation-Id` (lo genera el Gateway o este servicio si no viene).
  Se devuelve en la respuesta, se reenvía en las llamadas HTTP salientes y se lee del header del mensaje de Kafka.
  En Seq filtra por `CorrelationId = '...'` para ver el recorrido completo de una compra/venta entre servicios.
- **Errores no controlados:** `GlobalExceptionHandler` los registra como Error y responde 500 con
  `{ "isSuccess": false, "message": "Ocurrió un error interno. Código de seguimiento: <correlationId>" }`.

## Configuración

| Clave | Descripción |
|---|---|
| `ConnectionStrings:CadenaSQL` | Cadena de conexión a `DB_Inventory` |
| `Jwt:Authority` | URL de Auth, de donde se descarga la clave pública (local: `https://localhost:7186`) |
| `Jwt:RequireHttpsMetadata` | `false` solo dentro de Docker (http) |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con los de Auth |
| `Services:GatewayUrl` | URL del Gateway (local: `https://localhost:7134`; Docker: `http://gateway:8080`) |
| `Kafka:Enabled` | `false` para arrancar sin Kafka (desactiva el consumer) |
| `Kafka:BootstrapServers` | Broker de Kafka (local con Docker: `localhost:29092`; dentro de Docker: `kafka:9092`) |
| `Kafka:GroupId` | Grupo de consumidores (por defecto `inventory-service`) |
| `Seq:ServerUrl` | Servidor de logs Seq (local: `http://localhost:5341`; Docker: `http://seq:80`) |
| `Serilog:MinimumLevel` | Nivel mínimo de log y overrides por categoría |

## Ejecutar solo este servicio

```bash
dotnet run --project Inventory --launch-profile https
```

Requiere **Auth** corriendo (para validar tokens) y **Kafka** (o `Kafka:Enabled=false`). Puedes levantar solo la
infraestructura desde el repo del Gateway:

```bash
docker compose up -d sqlserver kafka kafka-init kafka-ui seq
```

Para ver los mensajes que llegan, abre Kafka UI en http://localhost:8090 → tópico `movimientos-stock`.

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`).

| Migración | Contenido |
|---|---|
| `InventoryInit` | Tablas `PRODUCTO_STOCK` y `MOVIMIENTO_STOCK` |
| `AuditoriaYProcedimientos` | Columnas de auditoría, `FECHA` → `FECHA_CREACION` (renombrada, sin perder datos), tabla `HISTORIAL_PRECIO` y el SP `PA_REGISTRAR_MOVIMIENTO_STOCK` |

### Stored procedures en las migraciones
Los SP viven como archivos `.sql` en `Domain/Migrations/Scripts/` (incluidos en `Domain.dll` como *EmbeddedResource*).
La migración los ejecuta con `migrationBuilder.EjecutarScript("001_PA_REGISTRAR_MOVIMIENTO_STOCK.sql")`, que divide
el archivo por las líneas `GO` igual que SSMS. Los SP se crean con `CREATE OR ALTER PROCEDURE`.

**Para cambiar un SP en el futuro** (nunca edites un `.sql` que ya se aplicó, porque las bases existentes no volverían
a ejecutarlo):
1. Crea un archivo nuevo con el siguiente número, p. ej. `Domain/Migrations/Scripts/002_PA_REGISTRAR_MOVIMIENTO_STOCK_v2.sql`,
   con el SP completo (`CREATE OR ALTER PROCEDURE ...`).
2. Crea una migración: `dotnet ef migrations add CambioRegistrarMovimiento --project Domain --startup-project Inventory`.
3. En su `Up()` llama `migrationBuilder.EjecutarScript("002_...sql");` y en `Down()` vuelve a ejecutar el script anterior
   (`001_...sql`) para restaurar la versión previa.

Para cambios de tablas, crea la migración normalmente:

```bash
dotnet ef migrations add NombreMigracion --project Domain --startup-project Inventory
```

> Revisa siempre la migración generada: si renombras una propiedad, EF suele generar `DropColumn` + `AddColumn`
> (pierde datos). Cámbialo por `RenameColumn`, como en `AuditoriaYProcedimientos`.

## Docker

```bash
docker build -t supermercado-inventory .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.
