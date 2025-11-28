using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.Infrastructure.Repositories;

public class RepositorioTurma : RepositorioBase<Turma>, IRepositorioTurma
{
    public RepositorioTurma(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<IEnumerable<Turma>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IgrejaId == igrejaId)
            .Include(t => t.Professor)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Turma>> ObterAtivasPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IgrejaId == igrejaId && t.Ativa)
            .Include(t => t.Professor)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Turma>> ObterPorProfessorAsync(Guid professorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ProfessorId == professorId && t.Ativa)
            .Include(t => t.Professor)
            .Include(t => t.Igreja)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Turma?> ObterComAlunosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Id == id)
            .Include(t => t.Professor)
            .Include(t => t.Igreja)
            .Include(t => t.Alunos.Where(a => a.Ativo))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class RepositorioAluno : RepositorioBase<Aluno>, IRepositorioAluno
{
    public RepositorioAluno(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<IEnumerable<Aluno>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IgrejaId == igrejaId)
            .Include(a => a.Turma)
            .OrderBy(a => a.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Aluno>> ObterPorTurmaAsync(Guid turmaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TurmaId == turmaId)
            .Include(a => a.Turma)
            .OrderBy(a => a.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Aluno>> ObterAtivosPorTurmaAsync(Guid turmaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TurmaId == turmaId && a.Ativo)
            .Include(a => a.Turma)
            .OrderBy(a => a.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Aluno>> ObterAniversariantesDoMesAsync(Guid igrejaId, int mes, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IgrejaId == igrejaId && a.Ativo && a.DataNascimento.Month == mes)
            .Include(a => a.Turma)
            .OrderBy(a => a.DataNascimento.Day)
            .ThenBy(a => a.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Aluno?> ObterComPresencasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Id == id)
            .Include(a => a.Turma)
            .Include(a => a.Presencas.OrderByDescending(p => p.Data))
            .FirstOrDefaultAsync(cancellationToken);
    }
}