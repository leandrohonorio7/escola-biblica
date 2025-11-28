using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class PresencaConfiguration : IEntityTypeConfiguration<Presenca>
{
    public void Configure(EntityTypeBuilder<Presenca> builder)
    {
        builder.ToTable("Presencas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Data)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(p => p.StatusPresenca)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Observacoes)
            .HasMaxLength(500);

        builder.Property(p => p.TrouxeAmigo)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.AlunoId)
            .IsRequired();

        builder.Property(p => p.TurmaId)
            .IsRequired();

        // Propriedades de auditoria
        builder.Property(p => p.IgrejaId)
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.Property(p => p.CriadoPor)
            .IsRequired();

        builder.Property(p => p.AtualizadoEm);

        builder.Property(p => p.AtualizadoPor);

        // Relacionamentos
        builder.HasOne(p => p.Igreja)
            .WithMany()
            .HasForeignKey(p => p.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Aluno)
            .WithMany(a => a.Presencas)
            .HasForeignKey(p => p.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Turma)
            .WithMany(t => t.Presencas)
            .HasForeignKey(p => p.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(p => new { p.AlunoId, p.Data })
            .IsUnique();

        builder.HasIndex(p => new { p.TurmaId, p.Data });

        builder.HasIndex(p => new { p.IgrejaId, p.Data });

        builder.HasIndex(p => p.StatusPresenca);

        builder.HasIndex(p => p.TrouxeAmigo);
    }
}