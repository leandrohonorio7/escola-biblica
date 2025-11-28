using EscolaBiblica.Domain.Common;
using EscolaBiblica.Domain.Enums;

namespace EscolaBiblica.Domain.Entities;

public class RegraCompetitiva : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public TipoRegra Tipo { get; private set; }
    public string ParametrosJson { get; private set; } = "{}";
    public bool Ativa { get; private set; } = true;
    public int Ordem { get; private set; }

    // Relacionamentos
    public Guid CompetitivaId { get; private set; }
    public virtual Competitiva Competitiva { get; private set; } = null!;
    public virtual Igreja Igreja { get; private set; } = null!;

    protected RegraCompetitiva() { }

    public RegraCompetitiva(string nome, TipoRegra tipo, string parametrosJson, int ordem,
        Guid competitivaId, Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Tipo = tipo;
        ParametrosJson = parametrosJson ?? "{}";
        Ordem = ordem >= 0 ? ordem : throw new ArgumentOutOfRangeException(nameof(ordem));
        CompetitivaId = competitivaId;
    }

    public void AtualizarRegra(string nome, string? descricao, string parametrosJson, 
        int ordem, Guid atualizadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        ParametrosJson = parametrosJson ?? "{}";
        Ordem = ordem >= 0 ? ordem : throw new ArgumentOutOfRangeException(nameof(ordem));
        Atualizar(atualizadoPor);
    }

    public void Ativar(Guid atualizadoPor)
    {
        Ativa = true;
        Atualizar(atualizadoPor);
    }

    public void Desativar(Guid atualizadoPor)
    {
        Ativa = false;
        Atualizar(atualizadoPor);
    }

    public void AlterarOrdem(int novaOrdem, Guid atualizadoPor)
    {
        Ordem = novaOrdem >= 0 ? novaOrdem : throw new ArgumentOutOfRangeException(nameof(novaOrdem));
        Atualizar(atualizadoPor);
    }
}