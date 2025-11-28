using EscolaBiblica.Domain.Common;

namespace EscolaBiblica.Domain.Entities;

public class Igreja : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public string? Endereco { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? CoresPersonalizadas { get; private set; } // JSON
    public string TimezoneId { get; private set; } = "E. South America Standard Time";
    public bool Ativa { get; private set; } = true;
    
    // Configurações de competição
    public bool PermiteCompetitivas { get; private set; } = true;
    public string? ConfiguracaoCompetitiva { get; private set; } // JSON

    // Relacionamentos
    public virtual ICollection<Usuario> Usuarios { get; private set; } = [];
    public virtual ICollection<Turma> Turmas { get; private set; } = [];
    public virtual ICollection<Competitiva> Competitivas { get; private set; } = [];

    protected Igreja() { }

    public Igreja(string nome, Guid criadoPor) : base(Guid.NewGuid(), criadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        IgrejaId = Id; // Para Igreja, o IgrejaId é o próprio Id
    }

    public void AtualizarInformacoes(string nome, string? descricao, string? endereco, 
        string? telefone, string? email, Guid atualizadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        Endereco = endereco;
        Telefone = telefone;
        Email = email;
        Atualizar(atualizadoPor);
    }

    public void ConfigurarVisualizacao(string? logoUrl, string? coresPersonalizadas, Guid atualizadoPor)
    {
        LogoUrl = logoUrl;
        CoresPersonalizadas = coresPersonalizadas;
        Atualizar(atualizadoPor);
    }

    public void ConfigurarCompetitivas(bool permiteCompetitivas, string? configuracao, Guid atualizadoPor)
    {
        PermiteCompetitivas = permiteCompetitivas;
        ConfiguracaoCompetitiva = configuracao;
        Atualizar(atualizadoPor);
    }

    public void DefinirTimezone(string timezoneId, Guid atualizadoPor)
    {
        TimezoneId = timezoneId ?? throw new ArgumentNullException(nameof(timezoneId));
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
}