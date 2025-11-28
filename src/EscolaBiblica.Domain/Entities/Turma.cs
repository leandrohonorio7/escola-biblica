using EscolaBiblica.Domain.Common;

namespace EscolaBiblica.Domain.Entities;

public class Turma : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int IdadeMinima { get; private set; }
    public int IdadeMaxima { get; private set; }
    public TimeSpan HorarioInicio { get; private set; }
    public TimeSpan HorarioFim { get; private set; }
    public string? Sala { get; private set; }
    public bool Ativa { get; private set; } = true;

    // Professor responsável
    public Guid? ProfessorId { get; private set; }

    // Relacionamentos
    public virtual Igreja Igreja { get; private set; } = null!;
    public virtual Usuario? Professor { get; private set; }
    public virtual ICollection<Aluno> Alunos { get; private set; } = [];
    public virtual ICollection<Presenca> Presencas { get; private set; } = [];

    protected Turma() { }

    public Turma(string nome, int idadeMinima, int idadeMaxima, TimeSpan horarioInicio, 
        TimeSpan horarioFim, Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        IdadeMinima = idadeMinima >= 0 ? idadeMinima : throw new ArgumentOutOfRangeException(nameof(idadeMinima));
        IdadeMaxima = idadeMaxima >= idadeMinima ? idadeMaxima : throw new ArgumentOutOfRangeException(nameof(idadeMaxima));
        HorarioInicio = horarioInicio;
        HorarioFim = horarioFim > horarioInicio ? horarioFim : throw new ArgumentException("Horário fim deve ser maior que horário início");
    }

    public void AtualizarInformacoes(string nome, string? descricao, int idadeMinima, 
        int idadeMaxima, TimeSpan horarioInicio, TimeSpan horarioFim, string? sala, Guid atualizadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao;
        IdadeMinima = idadeMinima >= 0 ? idadeMinima : throw new ArgumentOutOfRangeException(nameof(idadeMinima));
        IdadeMaxima = idadeMaxima >= idadeMinima ? idadeMaxima : throw new ArgumentOutOfRangeException(nameof(idadeMaxima));
        HorarioInicio = horarioInicio;
        HorarioFim = horarioFim > horarioInicio ? horarioFim : throw new ArgumentException("Horário fim deve ser maior que horário início");
        Sala = sala;
        Atualizar(atualizadoPor);
    }

    public void AtribuirProfessor(Guid professorId, Guid atualizadoPor)
    {
        ProfessorId = professorId;
        Atualizar(atualizadoPor);
    }

    public void RemoverProfessor(Guid atualizadoPor)
    {
        ProfessorId = null;
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

    public bool PodeReceberAluno(DateTime dataNascimento)
    {
        var idade = DateTime.Today.Year - dataNascimento.Year;
        if (dataNascimento.Date > DateTime.Today.AddYears(-idade))
            idade--;

        return idade >= IdadeMinima && idade <= IdadeMaxima;
    }

    public int QuantidadeAlunosMatriculados() => Alunos.Count(a => a.Ativo);

    public int CalcularPontosCompetitiva(DateTime periodo)
    {
        var presencasNoPeriodo = Presencas
            .Where(p => p.Data.Date == periodo.Date && p.StatusPresenca == Enums.StatusPresenca.Presente)
            .Count();

        var alunosMatriculados = QuantidadeAlunosMatriculados();
        
        if (alunosMatriculados == 0) return 0;

        // Pontuação proporcional conforme regulamento
        return (presencasNoPeriodo * 10) / alunosMatriculados;
    }
}