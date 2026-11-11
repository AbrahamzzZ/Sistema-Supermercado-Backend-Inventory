using Domain.Model.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public static class AuditoriaConfiguration
    {
        public static EntityTypeBuilder<T> ConfigurarAuditoria<T>(this EntityTypeBuilder<T> builder) where T : EntidadAuditable
        {
            builder.Property(e => e.IdUsuarioCreacion).HasColumnName("ID_USUARIO_CREACION");
            builder.Property(e => e.FechaCreacion)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("FECHA_CREACION");
            builder.Property(e => e.IdUsuarioModificacion).HasColumnName("ID_USUARIO_MODIFICACION");
            builder.Property(e => e.FechaModificacion)
                .HasColumnType("datetime2")
                .HasColumnName("FECHA_MODIFICACION");
            return builder;
        }
    }
}
