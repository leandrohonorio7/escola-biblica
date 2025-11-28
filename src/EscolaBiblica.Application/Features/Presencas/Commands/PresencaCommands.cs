using EscolaBiblica.Application.Common.Exceptions;
using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Enums;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using MediatR;

namespace EscolaBiblica.Application.Features.Presencas.Commands;

public record RegistrarPresencaCommand(Guid AlunoId, DateTime Data, StatusPresenca Status,
    bool TrouxeAmigo = false, string? Observacoes = null) : IRequest<Guid>;

public class RegistrarPresencaCommandHandler : IRequestHandler<RegistrarPresencaCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public RegistrarPresencaCommandHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<Guid> Handle(RegistrarPresencaCommand request, CancellationToken cancellationToken)
    {
        var criadoPor = _usuarioAtual.Id ?? throw new ForbiddenException("Usuário não autenticado");

        // Verificar se aluno existe
        var aluno = await _unitOfWork.Alunos.ObterPorIdAsync(request.AlunoId, cancellationToken);
        if (aluno == null)
            throw new NotFoundException("Aluno", request.AlunoId);

        if (!_usuarioAtual.PodeAcessarIgreja(aluno.IgrejaId))
            throw new ForbiddenException("Sem permissão para registrar presença deste aluno");

        if (!aluno.TurmaId.HasValue)
            throw new BadRequestException("Aluno deve estar matriculado em uma turma para ter presença registrada");

        // Verificar se já existe presença para este aluno nesta data
        var presencaExistente = await _unitOfWork.Presencas.ObterPorAlunoEDataAsync(
            request.AlunoId, request.Data, cancellationToken);

        if (presencaExistente != null)
        {
            // Atualizar presença existente
            presencaExistente.AlterarStatus(request.Status, criadoPor);
            presencaExistente.DefinirSeTrauxeAmigo(request.TrouxeAmigo, criadoPor);
            
            if (!string.IsNullOrEmpty(request.Observacoes))
                presencaExistente.AdicionarObservacao(request.Observacoes, criadoPor);

            await _unitOfWork.Presencas.AtualizarAsync(presencaExistente, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return presencaExistente.Id;
        }
        else
        {
            // Criar nova presença
            var presenca = new Presenca(request.Data, request.AlunoId, aluno.TurmaId.Value,
                request.Status, request.TrouxeAmigo, aluno.IgrejaId, criadoPor);

            if (!string.IsNullOrEmpty(request.Observacoes))
                presenca.AdicionarObservacao(request.Observacoes, criadoPor);

            await _unitOfWork.Presencas.AdicionarAsync(presenca, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return presenca.Id;
        }
    }
}

public record RegistrarPresencasTurmaCommand(Guid TurmaId, DateTime Data, 
    List<RegistroPresencaAluno> Presencas) : IRequest;

public record RegistroPresencaAluno(Guid AlunoId, StatusPresenca Status, bool TrouxeAmigo);

public class RegistrarPresencasTurmaCommandHandler : IRequestHandler<RegistrarPresencasTurmaCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public RegistrarPresencasTurmaCommandHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task Handle(RegistrarPresencasTurmaCommand request, CancellationToken cancellationToken)
    {
        var criadoPor = _usuarioAtual.Id ?? throw new ForbiddenException("Usuário não autenticado");

        // Verificar se turma existe
        var turma = await _unitOfWork.Turmas.ObterComAlunosAsync(request.TurmaId, cancellationToken);
        if (turma == null)
            throw new NotFoundException("Turma", request.TurmaId);

        if (!_usuarioAtual.PodeAcessarIgreja(turma.IgrejaId))
            throw new ForbiddenException("Sem permissão para registrar presenças nesta turma");

        // Obter presenças já existentes para esta data
        var presencasExistentes = await _unitOfWork.Presencas.ObterPorTurmaEDataAsync(
            request.TurmaId, request.Data, cancellationToken);

        foreach (var registroPresenca in request.Presencas)
        {
            // Verificar se aluno pertence à turma
            var aluno = turma.Alunos.FirstOrDefault(a => a.Id == registroPresenca.AlunoId);
            if (aluno == null)
                throw new BadRequestException($"Aluno {registroPresenca.AlunoId} não pertence à turma {turma.Nome}");

            var presencaExistente = presencasExistentes.FirstOrDefault(p => p.AlunoId == registroPresenca.AlunoId);

            if (presencaExistente != null)
            {
                // Atualizar presença existente
                presencaExistente.AlterarStatus(registroPresenca.Status, criadoPor);
                presencaExistente.DefinirSeTrauxeAmigo(registroPresenca.TrouxeAmigo, criadoPor);
                await _unitOfWork.Presencas.AtualizarAsync(presencaExistente, cancellationToken);
            }
            else
            {
                // Criar nova presença
                var novaPresenca = new Presenca(request.Data, registroPresenca.AlunoId, request.TurmaId,
                    registroPresenca.Status, registroPresenca.TrouxeAmigo, turma.IgrejaId, criadoPor);

                await _unitOfWork.Presencas.AdicionarAsync(novaPresenca, cancellationToken);
            }
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}