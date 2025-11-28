using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Enums;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EscolaBiblica.Infrastructure.Repositories;

public class RepositorioPresenca : RepositorioBase<Presenca>, IRepositorioPresenca
{
    public RepositorioPresenca(EscolaBiblicaDbContext context) : base(context) { }

    public async Task<IEnumerable<Presenca>> ObterPorTurmaEDataAsync(Guid turmaId, DateTime data, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TurmaId == turmaId && p.Data.Date == data.Date)
            .Include(p => p.Aluno)
            .OrderBy(p => p.Aluno.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Presenca>> ObterPorAlunoAsync(Guid alunoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AlunoId == alunoId)
            .Include(p => p.Turma)
            .OrderByDescending(p => p.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Presenca>> ObterPorPeriodoAsync(Guid igrejaId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IgrejaId == igrejaId && 
                       p.Data.Date >= dataInicio.Date && 
                       p.Data.Date <= dataFim.Date)
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .OrderByDescending(p => p.Data)
            .ThenBy(p => p.Turma.Nome)
            .ThenBy(p => p.Aluno.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Presenca?> ObterPorAlunoEDataAsync(Guid alunoId, DateTime data, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Aluno)
            .Include(p => p.Turma)
            .FirstOrDefaultAsync(p => p.AlunoId == alunoId && p.Data.Date == data.Date, cancellationToken);
    }

    public async Task<int> ContarPresencasDoMesAsync(Guid alunoId, DateTime mes, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(p => p.AlunoId == alunoId && 
                            p.Data.Year == mes.Year && 
                            p.Data.Month == mes.Month &&
                            p.StatusPresenca == StatusPresenca.Presente, 
                       cancellationToken);
    }
}