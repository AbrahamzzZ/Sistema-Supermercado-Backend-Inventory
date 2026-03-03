using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class ProductoStockConfiguration : IEntityTypeConfiguration<ProductoStock>
    {
        public void Configure(EntityTypeBuilder<ProductoStock> builder)
        {
            builder.ToTable("PRODUCTO_STOCK");
            builder.HasKey(e => e.IdProducto);

            builder.Property(e => e.IdProducto).HasColumnName("ID_PRODUCTO").ValueGeneratedNever(); ;
            builder.Property(e => e.Stock).HasColumnName("STOCK");
            builder.Property(e => e.PrecioCompra)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRECIO_COMPRA");
            builder.Property(e => e.PrecioVenta)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRECIO_VENTA");
        }
    }
}
