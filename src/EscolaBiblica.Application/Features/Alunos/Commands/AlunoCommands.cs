using EscolaBiblica.Application.Common.Exceptions;
using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using MediatR;

namespace EscolaBiblica.Application.Features.Alunos.Commands;

public record CriarAlunoCommand(string Nome, DateTime DataNascimento, Guid? TurmaId,
    string? NomeResponsavel, string? TelefoneResponsavel, string? EmailResponsavel,
    string? Endereco, string? Observacoes) : IRequest<Guid>;

public class CriarAlunoCommandHandler : IRequestHandler<CriarAlunoCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public CriarAlunoCommandHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<Guid> Handle(CriarAlunoCommand request, CancellationToken cancellationToken)
    {
        var igrejaId = _usuarioAtual.IgrejaId ?? throw new ForbiddenException("Usuário deve estar associado a uma igreja");
        var criadoPor = _usuarioAtual.Id ?? throw new ForbiddenException("Usuário não autenticado");

        // Verificar se turma existe e pertence à mesma igreja
        if (request.TurmaId.HasValue)
        {
            var turma = await _unitOfWork.Turmas.ObterPorIdAsync(request.TurmaId.Value, cancellationToken);
            if (turma == null)
                throw new NotFoundException("Turma", request.TurmaId.Value);
            
            if (!_usuarioAtual.PodeAcessarIgreja(turma.IgrejaId))
                throw new ForbiddenException("Sem permissão para matricular aluno nesta turma");

            // Verificar se aluno tem idade compatível com a turma
            if (!turma.PodeReceberAluno(request.DataNascimento))
                throw new BadRequestException($"Idade do aluno não é compatível com a turma {turma.Nome} (idade: {turma.IdadeMinima}-{turma.IdadeMaxima} anos)");
        }

        var aluno = new Aluno(request.Nome, request.DataNascimento, igrejaId, criadoPor);

        if (request.TurmaId.HasValue || !string.IsNullOrEmpty(request.NomeResponsavel) ||
            !string.IsNullOrEmpty(request.TelefoneResponsavel) || !string.IsNullOrEmpty(request.EmailResponsavel) ||
            !string.IsNullOrEmpty(request.Endereco) || !string.IsNullOrEmpty(request.Observacoes))
        {
            aluno.AtualizarInformacoes(request.Nome, request.DataNascimento, request.NomeResponsavel,
                request.TelefoneResponsavel, request.EmailResponsavel, request.Endereco, 
                request.Observacoes, criadoPor);

            if (request.TurmaId.HasValue)
                aluno.MatricularNaTurma(request.TurmaId.Value, criadoPor);
        }

        await _unitOfWork.Alunos.AdicionarAsync(aluno, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return aluno.Id;
    }
}

public record MatricularAlunoCommand(Guid AlunoId, Guid TurmaId) : IRequest;

public class MatricularAlunoCommandHandler : IRequestHandler<MatricularAlunoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public MatricularAlunoCommandHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task Handle(MatricularAlunoCommand request, CancellationToken cancellationToken)
    {
        var aluno = await _unitOfWork.Alunos.ObterPorIdAsync(request.AlunoId, cancellationToken);
        if (aluno == null)
            throw new NotFoundException("Aluno", request.AlunoId);

        var turma = await _unitOfWork.Turmas.ObterPorIdAsync(request.TurmaId, cancellationToken);
        if (turma == null)
            throw new NotFoundException("Turma", request.TurmaId);

        if (!_usuarioAtual.PodeAcessarIgreja(aluno.IgrejaId) || !_usuarioAtual.PodeAcessarIgreja(turma.IgrejaId))
            throw new ForbiddenException("Sem permissão para realizar esta operação");

        // Verificar compatibilidade de idade
        if (!turma.PodeReceberAluno(aluno.DataNascimento))
            throw new BadRequestException($"Idade do aluno não é compatível com a turma {turma.Nome}");

        var atualizadoPor = _usuarioAtual.Id!.Value;
        aluno.MatricularNaTurma(request.TurmaId, atualizadoPor);

        await _unitOfWork.Alunos.AtualizarAsync(aluno, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}