using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context
{
    public partial class InventorySistemaSupermercadoContext : DbContext
    {
        public virtual DbSet<ProductoStock> ProductoStocks { get; set; }

        public virtual DbSet<MovimientoStock> MoviemientoStocks { get; set; }


        public InventorySistemaSupermercadoContext(DbContextOptions<InventorySistemaSupermercadoContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventorySistemaSupermercadoContext).Assembly);
        }
    }
}



