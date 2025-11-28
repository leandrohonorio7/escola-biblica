using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class PontuacaoCompetitivaConfiguration : IEntityTypeConfiguration<PontuacaoCompetitiva>
{
    public void Configure(EntityTypeBuilder<PontuacaoCompetitiva> builder)
    {
        builder.ToTable("PontuacoesCompetitivas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Periodo)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(p => p.PontosIndividuais)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.PontosTurma)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.PontosExtras)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.DetalhesJson)
            .HasMaxLength(2000);

        builder.Property(p => p.CompetitivaId)
            .IsRequired();

        builder.Property(p => p.AlunoId);

        builder.Property(p => p.TurmaId);

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
        builder.HasOne(p => p.Competitiva)
            .WithMany(c => c.Pontuacoes)
            .HasForeignKey(p => p.CompetitivaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Aluno)
            .WithMany()
            .HasForeignKey(p => p.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Turma)
            .WithMany()
            .HasForeignKey(p => p.TurmaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Igreja)
            .WithMany()
            .HasForeignKey(p => p.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(p => new { p.CompetitivaId, p.AlunoId, p.Periodo })
            .IsUnique()
            .HasFilter("[AlunoId] IS NOT NULL");

        builder.HasIndex(p => new { p.CompetitivaId, p.TurmaId, p.Periodo })
            .IsUnique()
            .HasFilter("[TurmaId] IS NOT NULL");

        builder.HasIndex(p => new { p.CompetitivaId, p.Periodo });

        builder.HasIndex(p => new { p.CompetitivaId, p.PontosIndividuais, p.PontosTurma, p.PontosExtras });

        // Computed column para TotalPontos
        builder.Property(p => p.TotalPontos)
            .HasComputedColumnSql("[PontosIndividuais] + [PontosTurma] + [PontosExtras]", stored: true);

        // Constraint: deve ter AlunoId OU TurmaId, mas não ambos
        builder.HasCheckConstraint("CK_PontuacaoCompetitiva_AlunoOuTurma", 
            "([AlunoId] IS NOT NULL AND [TurmaId] IS NULL) OR ([AlunoId] IS NULL AND [TurmaId] IS NOT NULL)");
    }
}