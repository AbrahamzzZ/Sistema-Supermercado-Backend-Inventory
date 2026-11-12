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

Solo este servicio:

```bash
dotnet run --project Inventory --launch-profile https     # https://localhost:7091/swagger
```

Necesita SQL Server, **Auth** (para validar tokens) y **Kafka**, o `Kafka:Enabled=false` para arrancar sin el
consumidor. La infraestructura se puede levantar sola desde el repo del Gateway:

```bash
docker compose up -d sqlserver kafka kafka-init kafka-ui seq
```

Los mensajes que llegan se ven en Kafka UI (http://localhost:8090), tópico `movimientos-stock`.

## Endpoints principales

| Ruta (vía Gateway `/inventory/...`) | Descripción |
|---|---|
| `GET /api/ProductoStock` · `/{idProducto}` | Stock y últimos precios |
| `GET /api/ProductoStock/{idProducto}/historial-precios` | Historial de cambios de precio |
| `POST /api/MovimientoStock` | Movimiento manual (entrada o salida) |
