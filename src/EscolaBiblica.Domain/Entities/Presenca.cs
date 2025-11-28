using EscolaBiblica.Domain.Common;
using EscolaBiblica.Domain.Enums;

namespace EscolaBiblica.Domain.Entities;

public class Presenca : EntidadeBase
{
    public DateTime Data { get; private set; }
    public StatusPresenca StatusPresenca { get; private set; }
    public string? Observacoes { get; private set; }
    public bool TrouxeAmigo { get; private set; }

    // Relacionamentos
    public Guid AlunoId { get; private set; }
    public Guid TurmaId { get; private set; }
    public virtual Aluno Aluno { get; private set; } = null!;
    public virtual Turma Turma { get; private set; } = null!;
    public virtual Igreja Igreja { get; private set; } = null!;

    protected Presenca() { }

    public Presenca(DateTime data, Guid alunoId, Guid turmaId, StatusPresenca status, 
        bool trouxeAmigo, Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Data = data.Date; // Garantir que seja apenas a data
        AlunoId = alunoId;
        TurmaId = turmaId;
        StatusPresenca = status;
        TrouxeAmigo = trouxeAmigo;
    }

    public void AlterarStatus(StatusPresenca novoStatus, Guid atualizadoPor)
    {
        StatusPresenca = novoStatus;
        Atualizar(atualizadoPor);
    }

    public void DefinirSeTrauxeAmigo(bool trouxeAmigo, Guid atualizadoPor)
    {
        TrouxeAmigo = trouxeAmigo;
        Atualizar(atualizadoPor);
    }

    public void AdicionarObservacao(string observacoes, Guid atualizadoPor)
    {
        Observacoes = observacoes;
        Atualizar(atualizadoPor);
    }

    public bool EstaPresente() => StatusPresenca == StatusPresenca.Presente;

    public int CalcularPontosParaCompetitiva()
    {
        if (!EstaPresente()) return 0;

        var pontos = 1; // Ponto base por presença

        if (TrouxeAmigo)
            pontos += 1; // Ponto extra por trazer amigo

        return pontos;
    }
}