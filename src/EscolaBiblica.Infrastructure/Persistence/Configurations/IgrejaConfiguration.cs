using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class IgrejaConfiguration : IEntityTypeConfiguration<Igreja>
{
    public void Configure(EntityTypeBuilder<Igreja> builder)
    {
        builder.ToTable("Igrejas");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Descricao)
            .HasMaxLength(1000);

        builder.Property(i => i.Endereco)
            .HasMaxLength(500);

        builder.Property(i => i.Telefone)
            .HasMaxLength(20);

        builder.Property(i => i.Email)
            .HasMaxLength(200);

        builder.Property(i => i.LogoUrl)
            .HasMaxLength(500);

        builder.Property(i => i.CoresPersonalizadas)
            .HasMaxLength(2000);

        builder.Property(i => i.TimezoneId)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("E. South America Standard Time");

        builder.Property(i => i.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.PermiteCompetitivas)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.ConfiguracaoCompetitiva)
            .HasMaxLength(4000);

        // Propriedades de auditoria
        builder.Property(i => i.CriadoEm)
            .IsRequired();

        builder.Property(i => i.CriadoPor)
            .IsRequired();

        builder.Property(i => i.AtualizadoEm);

        builder.Property(i => i.AtualizadoPor);

        // Relacionamentos
        builder.HasMany(i => i.Usuarios)
            .WithOne(u => u.Igreja)
            .HasForeignKey(u => u.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Turmas)
            .WithOne(t => t.Igreja)
            .HasForeignKey(t => t.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Competitivas)
            .WithOne(c => c.Igreja)
            .HasForeignKey(c => c.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(i => i.Nome)
            .IsUnique();

        builder.HasIndex(i => i.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.HasIndex(i => i.Ativa);
    }
}