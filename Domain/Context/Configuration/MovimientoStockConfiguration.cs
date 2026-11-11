using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class MovimientoStockConfiguration : IEntityTypeConfiguration<MovimientoStock>
    {
        public void Configure(EntityTypeBuilder<MovimientoStock> builder)
        {
            builder.ToTable("MOVIMIENTO_STOCK");
            builder.HasKey(e => e.IdMovimientoStock);

            builder.Property(e => e.IdMovimientoStock).HasColumnName("ID_MOVIMIENTO");
            builder.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO");
            builder.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TIPO");
            builder.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
            builder.Property(e => e.Referencia)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REFERENCIA");

            builder.Property(e => e.IdUsuarioCreacion).HasColumnName("ID_USUARIO_CREACION");
            builder.Property(e => e.FechaCreacion)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("FECHA_CREACION");

            builder.HasIndex(e => new { e.IdProducto, e.FechaCreacion }, "IX_MOVIMIENTO_STOCK_PRODUCTO_FECHA");
        }
    }
}
