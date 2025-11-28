using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Enums;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.Infrastructure.Repositories;

public class RepositorioCompetitiva : RepositorioBase<Competitiva>, IRepositorioCompetitiva
{
    public RepositorioCompetitiva(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<IEnumerable<Competitiva>> ObterPorIgrejaAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IgrejaId == igrejaId)
            .OrderByDescending(c => c.DataInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Competitiva>> ObterAtivasAsync(Guid igrejaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IgrejaId == igrejaId && c.Status == StatusCompetitiva.Ativa)
            .OrderByDescending(c => c.DataInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<Competitiva?> ObterComRegrasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Id == id)
            .Include(c => c.Regras.Where(r => r.Ativa).OrderBy(r => r.Ordem))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Competitiva?> ObterAtivaNoPeriodoAsync(Guid igrejaId, DateTime data, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IgrejaId == igrejaId && 
                       c.Status == StatusCompetitiva.Ativa &&
                       c.DataInicio <= data.Date && 
                       c.DataFim >= data.Date)
            .Include(c => c.Regras.Where(r => r.Ativa).OrderBy(r => r.Ordem))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class RepositorioPontuacaoCompetitiva : RepositorioBase<PontuacaoCompetitiva>, IRepositorioPontuacaoCompetitiva
{
    public RepositorioPontuacaoCompetitiva(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<IEnumerable<PontuacaoCompetitiva>> ObterPorCompetitivaAsync(Guid competitivaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompetitivaId == competitivaId)
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .OrderByDescending(p => p.Periodo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PontuacaoCompetitiva>> ObterPorAlunoAsync(Guid alunoId, Guid competitivaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompetitivaId == competitivaId && p.AlunoId == alunoId)
            .Include(p => p.Aluno)
            .OrderByDescending(p => p.Periodo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PontuacaoCompetitiva>> ObterPorTurmaAsync(Guid turmaId, Guid competitivaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompetitivaId == competitivaId && p.TurmaId == turmaId)
            .Include(p => p.Turma)
            .OrderByDescending(p => p.Periodo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PontuacaoCompetitiva>> ObterRankingIndividualAsync(Guid competitivaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompetitivaId == competitivaId && p.AlunoId != null)
            .Include(p => p.Aluno)
                .ThenInclude(a => a!.Turma)
            .GroupBy(p => new { p.AlunoId, p.Aluno })
            .Select(g => new PontuacaoCompetitiva(
                DateTime.Today,
                competitivaId,
                g.Key.AlunoId!.Value,
                g.Sum(p => p.PontosIndividuais),
                g.Sum(p => p.PontosExtras),
                g.First().IgrejaId,
                g.First().CriadoPor
            ))
            .OrderByDescending(p => p.TotalPontos)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PontuacaoCompetitiva>> ObterRankingTurmasAsync(Guid competitivaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CompetitivaId == competitivaId && p.TurmaId != null)
            .Include(p => p.Turma)
            .GroupBy(p => new { p.TurmaId, p.Turma })
            .Select(g => new PontuacaoCompetitiva(
                DateTime.Today,
                competitivaId,
                g.Key.TurmaId!.Value,
                g.Sum(p => p.PontosTurma),
                g.First().IgrejaId,
                g.First().CriadoPor
            ))
            .OrderByDescending(p => p.TotalPontos)
            .ToListAsync(cancellationToken);
    }
}