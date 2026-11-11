using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class HistorialPrecioConfiguration : IEntityTypeConfiguration<HistorialPrecio>
    {
        public void Configure(EntityTypeBuilder<HistorialPrecio> builder)
        {
            builder.ToTable("HISTORIAL_PRECIO");
            builder.HasKey(e => e.IdHistorialPrecio);

            builder.Property(e => e.IdHistorialPrecio).HasColumnName("ID_HISTORIAL_PRECIO");
            builder.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO");
            builder.Property(e => e.PrecioCompraAnterior).HasColumnType("decimal(10, 2)").HasColumnName("PRECIO_COMPRA_ANTERIOR");
            builder.Property(e => e.PrecioCompraNuevo).HasColumnType("decimal(10, 2)").HasColumnName("PRECIO_COMPRA_NUEVO");
            builder.Property(e => e.PrecioVentaAnterior).HasColumnType("decimal(10, 2)").HasColumnName("PRECIO_VENTA_ANTERIOR");
            builder.Property(e => e.PrecioVentaNuevo).HasColumnType("decimal(10, 2)").HasColumnName("PRECIO_VENTA_NUEVO");
            builder.Property(e => e.Referencia)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REFERENCIA");
            builder.Property(e => e.IdUsuarioCreacion).HasColumnName("ID_USUARIO_CREACION");
            builder.Property(e => e.FechaCreacion)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("FECHA_CREACION");

            builder.HasIndex(e => new { e.IdProducto, e.FechaCreacion }, "IX_HISTORIAL_PRECIO_PRODUCTO_FECHA")
                .IsDescending(false, true);
        }
    }
}
