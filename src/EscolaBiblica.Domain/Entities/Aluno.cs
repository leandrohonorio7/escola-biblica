using EscolaBiblica.Domain.Common;

namespace EscolaBiblica.Domain.Entities;

public class Aluno : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public DateTime DataNascimento { get; private set; }
    public string? NomeResponsavel { get; private set; }
    public string? TelefoneResponsavel { get; private set; }
    public string? EmailResponsavel { get; private set; }
    public string? Endereco { get; private set; }
    public string? Observacoes { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Turma atual
    public Guid? TurmaId { get; private set; }

    // Relacionamentos
    public virtual Igreja Igreja { get; private set; } = null!;
    public virtual Turma? Turma { get; private set; }
    public virtual ICollection<Presenca> Presencas { get; private set; } = [];

    protected Aluno() { }

    public Aluno(string nome, DateTime dataNascimento, Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        DataNascimento = dataNascimento <= DateTime.Today ? dataNascimento : throw new ArgumentException("Data de nascimento não pode ser futura");
    }

    public void AtualizarInformacoes(string nome, DateTime dataNascimento, string? nomeResponsavel,
        string? telefoneResponsavel, string? emailResponsavel, string? endereco, 
        string? observacoes, Guid atualizadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        DataNascimento = dataNascimento <= DateTime.Today ? dataNascimento : throw new ArgumentException("Data de nascimento não pode ser futura");
        NomeResponsavel = nomeResponsavel;
        TelefoneResponsavel = telefoneResponsavel;
        EmailResponsavel = emailResponsavel;
        Endereco = endereco;
        Observacoes = observacoes;
        Atualizar(atualizadoPor);
    }

    public void MatricularNaTurma(Guid turmaId, Guid atualizadoPor)
    {
        TurmaId = turmaId;
        Atualizar(atualizadoPor);
    }

    public void RemoverDaTurma(Guid atualizadoPor)
    {
        TurmaId = null;
        Atualizar(atualizadoPor);
    }

    public void Ativar(Guid atualizadoPor)
    {
        Ativo = true;
        Atualizar(atualizadoPor);
    }

    public void Desativar(Guid atualizadoPor)
    {
        Ativo = false;
        Atualizar(atualizadoPor);
    }

    public int CalcularIdade()
    {
        var idade = DateTime.Today.Year - DataNascimento.Year;
        if (DataNascimento.Date > DateTime.Today.AddYears(-idade))
            idade--;
        return idade;
    }

    public bool EstaMatriculado() => TurmaId.HasValue && Ativo;

    public int CalcularPontosIndividuais(DateTime periodo, bool trouxeAmigo = false)
    {
        var pontos = 0;

        // 1 ponto base por estar matriculado
        if (EstaMatriculado())
            pontos += 1;

        // 1 ponto por assiduidade (presença)
        var presencaNoDia = Presencas.FirstOrDefault(p => p.Data.Date == periodo.Date);
        if (presencaNoDia?.StatusPresenca == Enums.StatusPresenca.Presente)
            pontos += 1;

        // 1 ponto extra por trazer amigo visitante
        if (trouxeAmigo)
            pontos += 1;

        return pontos;
    }

    public bool TemPresencaNoDia(DateTime data)
    {
        return Presencas.Any(p => p.Data.Date == data.Date && 
                                 p.StatusPresenca == Enums.StatusPresenca.Presente);
    }

    public int QuantidadePresencasNoMes(DateTime mes)
    {
        return Presencas.Count(p => p.Data.Year == mes.Year && 
                                   p.Data.Month == mes.Month &&
                                   p.StatusPresenca == Enums.StatusPresenca.Presente);
    }
}