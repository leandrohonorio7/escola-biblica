using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class TurmaConfiguration : IEntityTypeConfiguration<Turma>
{
    public void Configure(EntityTypeBuilder<Turma> builder)
    {
        builder.ToTable("Turmas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Descricao)
            .HasMaxLength(1000);

        builder.Property(t => t.IdadeMinima)
            .IsRequired();

        builder.Property(t => t.IdadeMaxima)
            .IsRequired();

        builder.Property(t => t.HorarioInicio)
            .IsRequired();

        builder.Property(t => t.HorarioFim)
            .IsRequired();

        builder.Property(t => t.Sala)
            .HasMaxLength(100);

        builder.Property(t => t.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.ProfessorId);

        // Propriedades de auditoria
        builder.Property(t => t.IgrejaId)
            .IsRequired();

        builder.Property(t => t.CriadoEm)
            .IsRequired();

        builder.Property(t => t.CriadoPor)
            .IsRequired();

        builder.Property(t => t.AtualizadoEm);

        builder.Property(t => t.AtualizadoPor);

        // Relacionamentos
        builder.HasOne(t => t.Igreja)
            .WithMany(i => i.Turmas)
            .HasForeignKey(t => t.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Professor)
            .WithMany(u => u.TurmasComoProfessor)
            .HasForeignKey(t => t.ProfessorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Alunos)
            .WithOne(a => a.Turma)
            .HasForeignKey(a => a.TurmaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Presencas)
            .WithOne(p => p.Turma)
            .HasForeignKey(p => p.TurmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(t => new { t.IgrejaId, t.Nome })
            .IsUnique();

        builder.HasIndex(t => t.ProfessorId);

        builder.HasIndex(t => t.Ativa);

        builder.HasIndex(t => new { t.IdadeMinima, t.IdadeMaxima });
    }
}