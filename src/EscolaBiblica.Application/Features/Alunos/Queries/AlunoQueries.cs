using AutoMapper;
using EscolaBiblica.Application.Common.Exceptions;
using EscolaBiblica.Application.Common.Mappings;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using MediatR;

namespace EscolaBiblica.Application.Features.Alunos.Queries;

public record ObterAlunosQuery(Guid? TurmaId = null) : IRequest<IEnumerable<AlunoDto>>;

public class ObterAlunosQueryHandler : IRequestHandler<ObterAlunosQuery, IEnumerable<AlunoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterAlunosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<IEnumerable<AlunoDto>> Handle(ObterAlunosQuery request, CancellationToken cancellationToken)
    {
        var igrejaId = _usuarioAtual.IgrejaId ?? throw new ForbiddenException("Usuário deve estar associado a uma igreja");

        IEnumerable<Domain.Entities.Aluno> alunos;

        if (request.TurmaId.HasValue)
        {
            // Verificar se turma pertence à igreja do usuário
            var turma = await _unitOfWork.Turmas.ObterPorIdAsync(request.TurmaId.Value, cancellationToken);
            if (turma == null)
                throw new NotFoundException("Turma", request.TurmaId.Value);

            if (!_usuarioAtual.PodeAcessarIgreja(turma.IgrejaId))
                throw new ForbiddenException("Sem permissão para acessar esta turma");

            alunos = await _unitOfWork.Alunos.ObterAtivosPorTurmaAsync(request.TurmaId.Value, cancellationToken);
        }
        else
        {
            alunos = await _unitOfWork.Alunos.ObterPorIgrejaAsync(igrejaId, cancellationToken);
        }

        return alunos.Select(a => new AlunoDto(
            a.Id,
            a.Nome, 
            a.DataNascimento,
            a.Turma?.Nome,
            a.Ativo,
            a.CalcularIdade()));
    }
}

public record ObterAlunoQuery(Guid Id) : IRequest<AlunoDto>;

public class ObterAlunoQueryHandler : IRequestHandler<ObterAlunoQuery, AlunoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterAlunoQueryHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<AlunoDto> Handle(ObterAlunoQuery request, CancellationToken cancellationToken)
    {
        var aluno = await _unitOfWork.Alunos.ObterPorIdAsync(request.Id, cancellationToken);
        if (aluno == null)
            throw new NotFoundException("Aluno", request.Id);

        if (!_usuarioAtual.PodeAcessarIgreja(aluno.IgrejaId))
            throw new ForbiddenException("Sem permissão para acessar este aluno");

        return new AlunoDto(
            aluno.Id,
            aluno.Nome,
            aluno.DataNascimento,
            aluno.Turma?.Nome,
            aluno.Ativo,
            aluno.CalcularIdade());
    }
}

public record ObterAniversariantesQuery(int Mes) : IRequest<IEnumerable<AlunoDto>>;

public class ObterAniversariantesQueryHandler : IRequestHandler<ObterAniversariantesQuery, IEnumerable<AlunoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterAniversariantesQueryHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<IEnumerable<AlunoDto>> Handle(ObterAniversariantesQuery request, CancellationToken cancellationToken)
    {
        var igrejaId = _usuarioAtual.IgrejaId ?? throw new ForbiddenException("Usuário deve estar associado a uma igreja");

        var alunos = await _unitOfWork.Alunos.ObterAniversariantesDoMesAsync(igrejaId, request.Mes, cancellationToken);

        return alunos.Select(a => new AlunoDto(
            a.Id,
            a.Nome,
            a.DataNascimento,
            a.Turma?.Nome,
            a.Ativo,
            a.CalcularIdade()));
    }
}