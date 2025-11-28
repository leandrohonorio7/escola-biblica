using AutoMapper;
using EscolaBiblica.Application.Common.Exceptions;
using EscolaBiblica.Application.Common.Interfaces;
using EscolaBiblica.Application.Common.Mappings;
using EscolaBiblica.Domain.Interfaces;
using MediatR;

namespace EscolaBiblica.Application.Features.Presencas.Queries;

public record ObterPresencasTurmaQuery(Guid TurmaId, DateTime Data) : IRequest<IEnumerable<PresencaDto>>;

public class ObterPresencasTurmaQueryHandler : IRequestHandler<ObterPresencasTurmaQuery, IEnumerable<PresencaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterPresencasTurmaQueryHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<IEnumerable<PresencaDto>> Handle(ObterPresencasTurmaQuery request, CancellationToken cancellationToken)
    {
        // Verificar se turma existe
        var turma = await _unitOfWork.Turmas.ObterPorIdAsync(request.TurmaId, cancellationToken);
        if (turma == null)
            throw new NotFoundException("Turma", request.TurmaId);

        if (!_usuarioAtual.PodeAcessarIgreja(turma.IgrejaId))
            throw new ForbiddenException("Sem permissão para acessar presenças desta turma");

        var presencas = await _unitOfWork.Presencas.ObterPorTurmaEDataAsync(
            request.TurmaId, request.Data, cancellationToken);

        return presencas.Select(p => new PresencaDto(
            p.Id,
            p.Data,
            p.StatusPresenca.ToString(),
            p.TrouxeAmigo,
            p.Aluno.Nome,
            p.Turma.Nome));
    }
}

public record ObterPresencasAlunoQuery(Guid AlunoId, DateTime? DataInicio = null, 
    DateTime? DataFim = null) : IRequest<IEnumerable<PresencaDto>>;

public class ObterPresencasAlunoQueryHandler : IRequestHandler<ObterPresencasAlunoQuery, IEnumerable<PresencaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterPresencasAlunoQueryHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<IEnumerable<PresencaDto>> Handle(ObterPresencasAlunoQuery request, CancellationToken cancellationToken)
    {
        // Verificar se aluno existe
        var aluno = await _unitOfWork.Alunos.ObterPorIdAsync(request.AlunoId, cancellationToken);
        if (aluno == null)
            throw new NotFoundException("Aluno", request.AlunoId);

        if (!_usuarioAtual.PodeAcessarIgreja(aluno.IgrejaId))
            throw new ForbiddenException("Sem permissão para acessar presenças deste aluno");

        var presencas = await _unitOfWork.Presencas.ObterPorAlunoAsync(request.AlunoId, cancellationToken);

        // Filtrar por período se especificado
        if (request.DataInicio.HasValue || request.DataFim.HasValue)
        {
            presencas = presencas.Where(p =>
                (!request.DataInicio.HasValue || p.Data.Date >= request.DataInicio.Value.Date) &&
                (!request.DataFim.HasValue || p.Data.Date <= request.DataFim.Value.Date));
        }

        return presencas.Select(p => new PresencaDto(
            p.Id,
            p.Data,
            p.StatusPresenca.ToString(),
            p.TrouxeAmigo,
            p.Aluno.Nome,
            p.Turma.Nome));
    }
}

public record ObterRelatorioPresencasQuery(DateTime DataInicio, DateTime DataFim, 
    Guid? TurmaId = null) : IRequest<RelatorioPresencasDto>;

public record RelatorioPresencasDto(
    int TotalPresencas,
    int TotalFaltas,
    double PercentualPresenca,
    int AmigosConvidados,
    IEnumerable<PresencaDto> Detalhes);

public class ObterRelatorioPresencasQueryHandler : IRequestHandler<ObterRelatorioPresencasQuery, RelatorioPresencasDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterRelatorioPresencasQueryHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<RelatorioPresencasDto> Handle(ObterRelatorioPresencasQuery request, CancellationToken cancellationToken)
    {
        var igrejaId = _usuarioAtual.IgrejaId ?? throw new ForbiddenException("Usuário deve estar associado a uma igreja");

        var presencas = await _unitOfWork.Presencas.ObterPorPeriodoAsync(
            igrejaId, request.DataInicio, request.DataFim, cancellationToken);

        // Filtrar por turma se especificado
        if (request.TurmaId.HasValue)
        {
            var turma = await _unitOfWork.Turmas.ObterPorIdAsync(request.TurmaId.Value, cancellationToken);
            if (turma == null)
                throw new NotFoundException("Turma", request.TurmaId.Value);

            if (!_usuarioAtual.PodeAcessarIgreja(turma.IgrejaId))
                throw new ForbiddenException("Sem permissão para acessar relatório desta turma");

            presencas = presencas.Where(p => p.TurmaId == request.TurmaId.Value);
        }

        var presencasLista = presencas.ToList();
        var totalPresencas = presencasLista.Count(p => p.StatusPresenca == Domain.Enums.StatusPresenca.Presente);
        var totalFaltas = presencasLista.Count(p => p.StatusPresenca == Domain.Enums.StatusPresenca.Ausente);
        var totalRegistros = totalPresencas + totalFaltas;
        var percentualPresenca = totalRegistros > 0 ? (double)totalPresencas / totalRegistros * 100 : 0;
        var amigosConvidados = presencasLista.Count(p => p.TrouxeAmigo);

        var detalhes = presencasLista.Select(p => new PresencaDto(
            p.Id,
            p.Data,
            p.StatusPresenca.ToString(),
            p.TrouxeAmigo,
            p.Aluno.Nome,
            p.Turma.Nome));

        return new RelatorioPresencasDto(
            totalPresencas,
            totalFaltas,
            Math.Round(percentualPresenca, 2),
            amigosConvidados,
            detalhes);
    }
}