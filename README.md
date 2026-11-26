# Sistema Supermercado — Inventory

Microservicio de **inventario**: stock y últimos precios de cada producto, historial de movimientos e
**historial de precios**. Recibe los movimientos de compras y ventas desde **Kafka** (publicados por Order) y
también movimientos manuales por HTTP.

**Base de datos:** `DB_Inventory` — `PRODUCTO_STOCK`, `MOVIMIENTO_STOCK`, `HISTORIAL_PRECIO`.

📘 Documentación:
- [Cómo levantar todo el sistema](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme) (README del Gateway)
- [Manual técnico](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway/blob/main/docs/Manual%20Tecnico.docx): capítulo 9 (Inventory) y 10 (Comunicación entre microservicios)

## Ejecutar

Normalmente se levanta con el `docker-compose.yml` del repo del Gateway (Swagger: http://localhost:8003/swagger).

**Para depurar (modo híbrido):** el resto del sistema sigue en Docker y este servicio corre desde Visual Studio.

```bash
# en el repo del Gateway
docker compose up -d                  # todo el sistema
docker compose stop inventory-api       # libera el servicio que vas a depurar
```

Luego F5 en Visual Studio (o `dotnet run --project Inventory --launch-profile https`) → https://localhost:7091/swagger.
En el entorno Development, `appsettings.Development.json` lo conecta a lo que corre en Docker: SQL Server
`localhost,14330` (las mismas bases), Auth `http://localhost:8002` (validación de tokens), Gateway
`http://localhost:8000` y Kafka `localhost:29092`.

Pruébalo desde su Swagger con un token de `POST http://localhost:8000/auth/Usuario/login` (botón *Authorize*):
el frontend sigue pasando por el Gateway de Docker, que apunta al contenedor detenido. Al terminar:
`docker compose up -d --build inventory-api`.

Para usar tu propio SQL Server en vez del de Docker, define `ConnectionStrings:CadenaSQL` con `dotnet user-secrets`
(tiene prioridad sobre `appsettings.Development.json`).

Con `Kafka:Enabled=false` arranca sin el consumidor. Mientras depuras, el contenedor de Inventory está detenido,
así que los mensajes de compras y ventas los consume tu instancia local.

Los mensajes que llegan se ven en Kafka UI (http://localhost:8090), tópico `movimientos-stock`.

## Endpoints principales

| Ruta (vía Gateway `/inventory/...`) | Descripción |
|---|---|
| `GET /api/ProductoStock` · `/{idProducto}` | Stock y últimos precios |
| `GET /api/ProductoStock/{idProducto}/historial-precios` | Historial de cambios de precio |
| `POST /api/MovimientoStock` | Movimiento manual (entrada o salida) |
