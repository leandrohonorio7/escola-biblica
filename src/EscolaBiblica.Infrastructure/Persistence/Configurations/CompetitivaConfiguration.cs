using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class CompetitivaConfiguration : IEntityTypeConfiguration<Competitiva>
{
    public void Configure(EntityTypeBuilder<Competitiva> builder)
    {
        builder.ToTable("Competitivas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Descricao)
            .HasMaxLength(1000);

        builder.Property(c => c.DataInicio)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(c => c.DataFim)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(1); // Ativa

        builder.Property(c => c.ConfiguracoesJson)
            .HasMaxLength(4000);

        // Propriedades de auditoria
        builder.Property(c => c.IgrejaId)
            .IsRequired();

        builder.Property(c => c.CriadoEm)
            .IsRequired();

        builder.Property(c => c.CriadoPor)
            .IsRequired();

        builder.Property(c => c.AtualizadoEm);

        builder.Property(c => c.AtualizadoPor);

        // Relacionamentos
        builder.HasOne(c => c.Igreja)
            .WithMany(i => i.Competitivas)
            .HasForeignKey(c => c.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Regras)
            .WithOne(r => r.Competitiva)
            .HasForeignKey(r => r.CompetitivaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Pontuacoes)
            .WithOne(p => p.Competitiva)
            .HasForeignKey(p => p.CompetitivaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => new { c.IgrejaId, c.Nome })
            .IsUnique();

        builder.HasIndex(c => c.Status);

        builder.HasIndex(c => new { c.DataInicio, c.DataFim });

        builder.HasIndex(c => new { c.IgrejaId, c.Status, c.DataInicio, c.DataFim });
    }
}

public class RegraCompetitivaConfiguration : IEntityTypeConfiguration<RegraCompetitiva>
{
    public void Configure(EntityTypeBuilder<RegraCompetitiva> builder)
    {
        builder.ToTable("RegrasCompetitivas");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Descricao)
            .HasMaxLength(1000);

        builder.Property(r => r.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.ParametrosJson)
            .IsRequired()
            .HasMaxLength(4000)
            .HasDefaultValue("{}");

        builder.Property(r => r.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Ordem)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.CompetitivaId)
            .IsRequired();

        // Propriedades de auditoria
        builder.Property(r => r.IgrejaId)
            .IsRequired();

        builder.Property(r => r.CriadoEm)
            .IsRequired();

        builder.Property(r => r.CriadoPor)
            .IsRequired();

        builder.Property(r => r.AtualizadoEm);

        builder.Property(r => r.AtualizadoPor);

        // Relacionamentos
        builder.HasOne(r => r.Competitiva)
            .WithMany(c => c.Regras)
            .HasForeignKey(r => r.CompetitivaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Igreja)
            .WithMany()
            .HasForeignKey(r => r.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(r => new { r.CompetitivaId, r.Ordem });

        builder.HasIndex(r => new { r.CompetitivaId, r.Tipo });

        builder.HasIndex(r => r.Ativa);
    }
}