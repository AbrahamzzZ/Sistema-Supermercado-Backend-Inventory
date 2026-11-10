# Sistema Supermercado — Inventory

Microservicio de **inventario**: mantiene el stock de cada producto y el historial de movimientos.
Recibe los movimientos de forma **asíncrona desde Kafka** (publicados por Order) y también por HTTP.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- Stock actual por producto (`PRODUCTO_STOCK`)
- Historial de entradas y salidas (`MOVIMIENTO_STOCK`)
- Consumir los eventos de compras y ventas para actualizar el stock

**Base de datos:** `DB_Inventory` — tablas `PRODUCTO_STOCK`, `MOVIMIENTO_STOCK`.

## Estructura

```
Inventory/       API (controllers, Program.cs, appsettings)
Domain/          Entidades, DTOs, DbContext y Migrations
Infrastructure/  Repositorio, servicios, validadores, Handler del token y Kafka (consumer)
Utilities/       ApiResponse, mensajes y paginación
```

## Endpoints

Vía Gateway: `http://localhost:8000/inventory/...` → aquí `/api/...` · **Requiere token.**

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/MovimientoStock` | Registra un movimiento manual (valida en Admin que el producto exista) |

```json
{ "idProducto": 5, "tipo": "ENTRADA", "cantidad": 10, "referencia": "AJUSTE", "precioCompra": 1.20, "precioVenta": 1.80 }
```

Swagger: http://localhost:8003/swagger (Docker) · https://localhost:7091/swagger (local)

## Integraciones

### Kafka (asíncrono) — consumer
`MovimientoStockConsumer` (un `BackgroundService`) escucha el tópico **`movimientos-stock`** que publica **Order**:

| Origen | `Tipo` | `Referencia` |
|---|---|---|
| Compra | `ENTRADA` | `COMPRA` |
| Venta | `SALIDA` | `VENTA` |

- **Grupo de consumidores:** `inventory-service`. Si levantas varias instancias, Kafka reparte las particiones entre ellas.
- **Confirmación manual:** el mensaje solo se marca como procesado cuando el movimiento se guardó en la base.
  Si falla (por ejemplo, SQL Server no disponible), se reintenta el mismo mensaje cada 5 s.
- Los mensajes con JSON inválido o que no pasan la validación se descartan y se registran en el log.
- No vuelve a validar el producto contra Admin: Order ya lo validó antes de publicar, y el consumer
  no tiene el token del usuario para llamar al Gateway.

### HTTP (síncrono) — a través del Gateway
El endpoint `POST /api/MovimientoStock` consulta a **Admin** (`/admin/Producto/{id}`) reenviando el token del usuario.

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

## Ejecutar solo este servicio

```bash
dotnet run --project Inventory --launch-profile https
```

Requiere **Auth** corriendo (para validar tokens) y **Kafka** (o `Kafka:Enabled=false`). Puedes levantar solo la
infraestructura desde el repo del Gateway:

```bash
docker compose up -d sqlserver kafka kafka-init kafka-ui
```

Para ver los mensajes que llegan, abre Kafka UI en http://localhost:8090 → tópico `movimientos-stock`.

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`). Para crear una nueva:

```bash
dotnet ef migrations add NombreMigracion --project Domain --startup-project Inventory
```

## Docker

```bash
docker build -t supermercado-inventory .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.
