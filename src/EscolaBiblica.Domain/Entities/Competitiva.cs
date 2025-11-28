using EscolaBiblica.Domain.Common;
using EscolaBiblica.Domain.Enums;

namespace EscolaBiblica.Domain.Entities;

public class Competitiva : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public StatusCompetitiva Status { get; private set; } = StatusCompetitiva.Ativa;
    public string? ConfiguracoesJson { get; private set; } // Configurações específicas

    // Relacionamentos
    public virtual Igreja Igreja { get; private set; } = null!;
    public virtual ICollection<RegraCompetitiva> Regras { get; private set; } = [];
    public virtual ICollection<PontuacaoCompetitiva> Pontuacoes { get; private set; } = [];

    protected Competitiva() { }

    public Competitiva(string nome, DateTime dataInicio, DateTime dataFim, 
        Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        DataInicio = dataInicio.Date;
        DataFim = dataFim.Date >= dataInicio.Date ? dataFim.Date : throw new ArgumentException("Data fim deve ser maior ou igual à data início");
    }

    public void AtualizarInformacoes(string nome, string? descricao, DateTime dataInicio, 
        DateTime dataFim, Guid atualizadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        DataInicio = dataInicio.Date;
        DataFim = dataFim.Date >= dataInicio.Date ? dataFim.Date : throw new ArgumentException("Data fim deve ser maior ou igual à data início");
        Atualizar(atualizadoPor);
    }

    public void AlterarStatus(StatusCompetitiva novoStatus, Guid atualizadoPor)
    {
        Status = novoStatus;
        Atualizar(atualizadoPor);
    }

    public void DefinirConfiguracoes(string configuracoes, Guid atualizadoPor)
    {
        ConfiguracoesJson = configuracoes;
        Atualizar(atualizadoPor);
    }

    public bool EstaAtiva() => Status == StatusCompetitiva.Ativa;

    public bool EstaNoPerido(DateTime data)
    {
        return data.Date >= DataInicio.Date && data.Date <= DataFim.Date;
    }

    public void Iniciar(Guid atualizadoPor)
    {
        if (Status != StatusCompetitiva.Pausada)
            throw new InvalidOperationException("Só é possível iniciar competições pausadas");
        
        Status = StatusCompetitiva.Ativa;
        Atualizar(atualizadoPor);
    }

    public void Pausar(Guid atualizadoPor)
    {
        if (Status != StatusCompetitiva.Ativa)
            throw new InvalidOperationException("Só é possível pausar competições ativas");
        
        Status = StatusCompetitiva.Pausada;
        Atualizar(atualizadoPor);
    }

    public void Finalizar(Guid atualizadoPor)
    {
        if (Status == StatusCompetitiva.Finalizada)
            throw new InvalidOperationException("Competição já está finalizada");
        
        Status = StatusCompetitiva.Finalizada;
        Atualizar(atualizadoPor);
    }

    public void Cancelar(Guid atualizadoPor)
    {
        if (Status == StatusCompetitiva.Finalizada)
            throw new InvalidOperationException("Não é possível cancelar uma competição finalizada");
        
        Status = StatusCompetitiva.Cancelada;
        Atualizar(atualizadoPor);
    }
}