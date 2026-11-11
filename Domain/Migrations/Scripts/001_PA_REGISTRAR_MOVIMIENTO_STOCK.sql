-- =====================================================================
-- 001 - PA_REGISTRAR_MOVIMIENTO_STOCK
-- Registra una ENTRADA (compra) o SALIDA (venta) de stock, con auditoría
-- e historial de precios, todo en una sola transacción.
--
-- Errores de NEGOCIO (no se deben reintentar):
--   50001 -> Stock insuficiente para el producto.
--   50002 -> Tipo de movimiento inválido.
-- =====================================================================
CREATE OR ALTER PROCEDURE PA_REGISTRAR_MOVIMIENTO_STOCK
(
    @ID_PRODUCTO   INT,
    @TIPO          VARCHAR(20),          -- 'ENTRADA' | 'SALIDA'
    @CANTIDAD      INT,
    @PRECIO_COMPRA DECIMAL(10,2) = NULL,
    @PRECIO_VENTA  DECIMAL(10,2) = NULL,
    @REFERENCIA    VARCHAR(50),          -- 'COMPRA' | 'VENTA' | 'AJUSTE' ...
    @ID_USUARIO    INT = NULL            -- usuario que originó el movimiento (auditoría)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @TIPO NOT IN ('ENTRADA', 'SALIDA')
            THROW 50002, 'Tipo de movimiento inválido. Debe ser ENTRADA o SALIDA.', 1;

        BEGIN TRANSACTION;

        DECLARE @STOCK_ACTUAL INT,
                @PRECIO_COMPRA_ANTERIOR DECIMAL(10,2),
                @PRECIO_VENTA_ANTERIOR  DECIMAL(10,2);

        -- UPDLOCK/HOLDLOCK: evita que dos movimientos simultáneos del mismo producto lean el mismo stock
        SELECT @STOCK_ACTUAL = STOCK,
               @PRECIO_COMPRA_ANTERIOR = PRECIO_COMPRA,
               @PRECIO_VENTA_ANTERIOR  = PRECIO_VENTA
        FROM PRODUCTO_STOCK WITH (UPDLOCK, HOLDLOCK)
        WHERE ID_PRODUCTO = @ID_PRODUCTO;

        -- 1. Crear registro de stock si no existe
        IF @STOCK_ACTUAL IS NULL
        BEGIN
            INSERT INTO PRODUCTO_STOCK (ID_PRODUCTO, STOCK, PRECIO_COMPRA, PRECIO_VENTA, ID_USUARIO_CREACION)
            VALUES (@ID_PRODUCTO, 0, NULL, NULL, @ID_USUARIO);

            SET @STOCK_ACTUAL = 0;
        END

        -- 2. Validar stock solo si es SALIDA
        IF @TIPO = 'SALIDA' AND @STOCK_ACTUAL < @CANTIDAD
            THROW 50001, 'Stock insuficiente para el producto.', 1;

        -- 3. Calcular precios nuevos (solo una ENTRADA puede cambiar precios)
        DECLARE @PRECIO_COMPRA_NUEVO DECIMAL(10,2) =
                    CASE WHEN @TIPO = 'ENTRADA' AND @PRECIO_COMPRA IS NOT NULL THEN @PRECIO_COMPRA ELSE @PRECIO_COMPRA_ANTERIOR END,
                @PRECIO_VENTA_NUEVO DECIMAL(10,2) =
                    CASE WHEN @TIPO = 'ENTRADA' AND @PRECIO_VENTA IS NOT NULL THEN @PRECIO_VENTA ELSE @PRECIO_VENTA_ANTERIOR END;

        -- 4. Actualizar stock, precios y auditoría
        UPDATE PRODUCTO_STOCK
        SET STOCK = CASE WHEN @TIPO = 'ENTRADA' THEN STOCK + @CANTIDAD ELSE STOCK - @CANTIDAD END,
            PRECIO_COMPRA = @PRECIO_COMPRA_NUEVO,
            PRECIO_VENTA  = @PRECIO_VENTA_NUEVO,
            ID_USUARIO_MODIFICACION = @ID_USUARIO,
            FECHA_MODIFICACION      = SYSUTCDATETIME()
        WHERE ID_PRODUCTO = @ID_PRODUCTO;

        -- 5. Historial de precios: solo si cambió algún precio (incluye la primera vez que tiene precio)
        IF @TIPO = 'ENTRADA'
           AND (   (@PRECIO_COMPRA_NUEVO IS NOT NULL AND (@PRECIO_COMPRA_ANTERIOR IS NULL OR @PRECIO_COMPRA_NUEVO <> @PRECIO_COMPRA_ANTERIOR))
                OR (@PRECIO_VENTA_NUEVO  IS NOT NULL AND (@PRECIO_VENTA_ANTERIOR  IS NULL OR @PRECIO_VENTA_NUEVO  <> @PRECIO_VENTA_ANTERIOR)))
        BEGIN
            INSERT INTO HISTORIAL_PRECIO
                (ID_PRODUCTO, PRECIO_COMPRA_ANTERIOR, PRECIO_COMPRA_NUEVO, PRECIO_VENTA_ANTERIOR, PRECIO_VENTA_NUEVO, REFERENCIA, ID_USUARIO_CREACION)
            VALUES
                (@ID_PRODUCTO, @PRECIO_COMPRA_ANTERIOR, @PRECIO_COMPRA_NUEVO, @PRECIO_VENTA_ANTERIOR, @PRECIO_VENTA_NUEVO, @REFERENCIA, @ID_USUARIO);
        END

        -- 6. Registrar movimiento (inmutable)
        INSERT INTO MOVIMIENTO_STOCK (ID_PRODUCTO, TIPO, CANTIDAD, REFERENCIA, ID_USUARIO_CREACION)
        VALUES (@ID_PRODUCTO, @TIPO, @CANTIDAD, @REFERENCIA, @ID_USUARIO);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;  -- relanza el error original (conserva el número 50001/50002)
    END CATCH
END;
GO
