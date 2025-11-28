using EscolaBiblica.Domain.Entities;

namespace EscolaBiblica.Domain.Interfaces;

public interface IRepositorioIgreja : IRepositorioBase<Igreja>
{
    Task<Igreja?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<IEnumerable<Igreja>> ObterAtivasAsync(CancellationToken cancellationToken = default);
    Task<bool> NomeJaExisteAsync(string nome, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}

public interface IRepositorioUsuario : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Usuario>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Usuario>> ObterProfessoresAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExisteAsync(string email, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}

public interface IRepositorioTurma : IRepositorioBase<Turma>
{
    Task<IEnumerable<Turma>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Turma>> ObterAtivasPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Turma>> ObterPorProfessorAsync(Guid professorId, CancellationToken cancellationToken = default);
    Task<Turma?> ObterComAlunosAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRepositorioAluno : IRepositorioBase<Aluno>
{
    Task<IEnumerable<Aluno>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterPorTurmaAsync(Guid turmaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterAtivosPorTurmaAsync(Guid turmaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterAniversariantesDoMesAsync(Guid igrejaId, int mes, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterComPresencasAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRepositorioPresenca : IRepositorioBase<Presenca>
{
    Task<IEnumerable<Presenca>> ObterPorTurmaEDataAsync(Guid turmaId, DateTime data, CancellationToken cancellationToken = default);
    Task<IEnumerable<Presenca>> ObterPorAlunoAsync(Guid alunoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Presenca>> ObterPorPeriodoAsync(Guid igrejaId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<Presenca?> ObterPorAlunoEDataAsync(Guid alunoId, DateTime data, CancellationToken cancellationToken = default);
    Task<int> ContarPresencasDoMesAsync(Guid alunoId, DateTime mes, CancellationToken cancellationToken = default);
}

public interface IRepositorioCompetitiva : IRepositorioBase<Competitiva>
{
    Task<IEnumerable<Competitiva>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Competitiva>> ObterAtivasAsync(Guid igrejaId, CancellationToken cancellationToken = default);
    Task<Competitiva?> ObterComRegrasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Competitiva?> ObterAtivaNoPeriodoAsync(Guid igrejaId, DateTime data, CancellationToken cancellationToken = default);
}

public interface IRepositorioPontuacaoCompetitiva : IRepositorioBase<PontuacaoCompetitiva>
{
    Task<IEnumerable<PontuacaoCompetitiva>> ObterPorCompetitivaAsync(Guid competitivaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PontuacaoCompetitiva>> ObterPorAlunoAsync(Guid alunoId, Guid competitivaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PontuacaoCompetitiva>> ObterPorTurmaAsync(Guid turmaId, Guid competitivaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PontuacaoCompetitiva>> ObterRankingIndividualAsync(Guid competitivaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PontuacaoCompetitiva>> ObterRankingTurmasAsync(Guid competitivaId, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    IRepositorioIgreja Igrejas { get; }
    IRepositorioUsuario Usuarios { get; }
    IRepositorioTurma Turmas { get; }
    IRepositorioAluno Alunos { get; }
    IRepositorioPresenca Presencas { get; }
    IRepositorioCompetitiva Competitivas { get; }
    IRepositorioPontuacaoCompetitiva PontuacoesCompetitivas { get; }
    
    Task<int> SalvarMudancasAsync(CancellationToken cancellationToken = default);
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}