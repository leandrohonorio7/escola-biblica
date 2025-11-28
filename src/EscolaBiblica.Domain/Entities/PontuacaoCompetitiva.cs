using EscolaBiblica.Domain.Common;

namespace EscolaBiblica.Domain.Entities;

public class PontuacaoCompetitiva : EntidadeBase
{
    public DateTime Periodo { get; private set; }
    public int PontosIndividuais { get; private set; }
    public int PontosTurma { get; private set; }
    public int PontosExtras { get; private set; }
    public int TotalPontos => PontosIndividuais + PontosTurma + PontosExtras;
    public string? DetalhesJson { get; private set; } // Detalhes do cálculo

    // Relacionamentos
    public Guid CompetitivaId { get; private set; }
    public Guid? AlunoId { get; private set; } // Para pontuação individual
    public Guid? TurmaId { get; private set; } // Para pontuação de turma
    
    public virtual Competitiva Competitiva { get; private set; } = null!;
    public virtual Aluno? Aluno { get; private set; }
    public virtual Turma? Turma { get; private set; }
    public virtual Igreja Igreja { get; private set; } = null!;

    protected PontuacaoCompetitiva() { }

    // Construtor para pontuação individual
    public PontuacaoCompetitiva(DateTime periodo, Guid competitivaId, Guid alunoId, 
        int pontosIndividuais, int pontosExtras, Guid igrejaId, Guid criadoPor) 
        : base(igrejaId, criadoPor)
    {
        Periodo = periodo.Date;
        CompetitivaId = competitivaId;
        AlunoId = alunoId;
        PontosIndividuais = pontosIndividuais >= 0 ? pontosIndividuais : 0;
        PontosExtras = pontosExtras >= 0 ? pontosExtras : 0;
        PontosTurma = 0;
    }

    // Construtor para pontuação de turma
    public PontuacaoCompetitiva(DateTime periodo, Guid competitivaId, Guid turmaId, 
        int pontosTurma, Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Periodo = periodo.Date;
        CompetitivaId = competitivaId;
        TurmaId = turmaId;
        PontosTurma = pontosTurma >= 0 ? pontosTurma : 0;
        PontosIndividuais = 0;
        PontosExtras = 0;
    }

    public void AtualizarPontos(int pontosIndividuais, int pontosTurma, int pontosExtras, 
        string? detalhes, Guid atualizadoPor)
    {
        PontosIndividuais = pontosIndividuais >= 0 ? pontosIndividuais : 0;
        PontosTurma = pontosTurma >= 0 ? pontosTurma : 0;
        PontosExtras = pontosExtras >= 0 ? pontosExtras : 0;
        DetalhesJson = detalhes;
        Atualizar(atualizadoPor);
    }

    public void DefinirDetalhesCalculo(string detalhes, Guid atualizadoPor)
    {
        DetalhesJson = detalhes;
        Atualizar(atualizadoPor);
    }

    public bool EhPontuacaoIndividual() => AlunoId.HasValue;
    
    public bool EhPontuacaoTurma() => TurmaId.HasValue;
}