using EscolaBiblica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EscolaBiblica.Infrastructure.Persistence.Configurations;

public class AlunoConfiguration : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("Alunos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.DataNascimento)
            .IsRequired();

        builder.Property(a => a.NomeResponsavel)
            .HasMaxLength(200);

        builder.Property(a => a.TelefoneResponsavel)
            .HasMaxLength(20);

        builder.Property(a => a.EmailResponsavel)
            .HasMaxLength(200);

        builder.Property(a => a.Endereco)
            .HasMaxLength(500);

        builder.Property(a => a.Observacoes)
            .HasMaxLength(2000);

        builder.Property(a => a.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.TurmaId);

        // Propriedades de auditoria
        builder.Property(a => a.IgrejaId)
            .IsRequired();

        builder.Property(a => a.CriadoEm)
            .IsRequired();

        builder.Property(a => a.CriadoPor)
            .IsRequired();

        builder.Property(a => a.AtualizadoEm);

        builder.Property(a => a.AtualizadoPor);

        // Relacionamentos
        builder.HasOne(a => a.Igreja)
            .WithMany()
            .HasForeignKey(a => a.IgrejaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Turma)
            .WithMany(t => t.Alunos)
            .HasForeignKey(a => a.TurmaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Presencas)
            .WithOne(p => p.Aluno)
            .HasForeignKey(p => p.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(a => new { a.IgrejaId, a.Nome });

        builder.HasIndex(a => a.TurmaId);

        builder.HasIndex(a => a.Ativo);

        builder.HasIndex(a => a.DataNascimento);

        builder.HasIndex(a => a.EmailResponsavel)
            .HasFilter("[EmailResponsavel] IS NOT NULL");
    }
}