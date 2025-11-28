using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Telefone)
            .HasMaxLength(20);

        builder.Property(u => u.DataNascimento);

        builder.Property(u => u.Perfil)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.UltimoAcesso);

        builder.Property(u => u.IdentityUserId)
            .IsRequired()
            .HasMaxLength(450);

        // Propriedades de auditoria
        builder.Property(u => u.IgrejaId)
            .IsRequired();

        builder.Property(u => u.CriadoEm)
            .IsRequired();

        builder.Property(u => u.CriadoPor)
            .IsRequired();

        builder.Property(u => u.AtualizadoEm);

        builder.Property(u => u.AtualizadoPor);

        // Relacionamentos
        builder.HasOne(u => u.Igreja)
            .WithMany(i => i.Usuarios)
            .HasForeignKey(u => u.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.TurmasComoProfessor)
            .WithOne(t => t.Professor)
            .HasForeignKey(t => t.ProfessorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índices
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.IdentityUserId)
            .IsUnique();

        builder.HasIndex(u => new { u.IgrejaId, u.Perfil });

        builder.HasIndex(u => u.Ativo);
    }
}