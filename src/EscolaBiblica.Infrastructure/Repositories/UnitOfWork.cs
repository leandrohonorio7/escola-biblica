using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;

namespace EscolaBiblica.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EscolaBiblicaDbContext _context;
    
    private IRepositorioIgreja? _igrejas;
    private IRepositorioUsuario? _usuarios;
    private IRepositorioTurma? _turmas;
    private IRepositorioAluno? _alunos;
    private IRepositorioPresenca? _presencas;
    private IRepositorioCompetitiva? _competitivas;
    private IRepositorioPontuacaoCompetitiva? _pontuacoesCompetitivas;

    public UnitOfWork(EscolaBiblicaDbContext context)
    {
        _context = context;
    }

    public IRepositorioIgreja Igrejas
    {
        get { return _igrejas ??= new RepositorioIgreja(_context); }
    }

    public IRepositorioUsuario Usuarios
    {
        get { return _usuarios ??= new RepositorioUsuario(_context); }
    }

    public IRepositorioTurma Turmas
    {
        get { return _turmas ??= new RepositorioTurma(_context); }
    }

    public IRepositorioAluno Alunos
    {
        get { return _alunos ??= new RepositorioAluno(_context); }
    }

    public IRepositorioPresenca Presencas
    {
        get { return _presencas ??= new RepositorioPresenca(_context); }
    }

    public IRepositorioCompetitiva Competitivas
    {
        get { return _competitivas ??= new RepositorioCompetitiva(_context); }
    }

    public IRepositorioPontuacaoCompetitiva PontuacoesCompetitivas
    {
        get { return _pontuacoesCompetitivas ??= new RepositorioPontuacaoCompetitiva(_context); }
    }

    public async Task<int> SalvarMudancasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var resultado = await SalvarMudancasAsync(cancellationToken);
            return resultado > 0;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}