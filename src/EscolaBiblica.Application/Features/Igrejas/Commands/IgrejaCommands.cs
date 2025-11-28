using EscolaBiblica.Application.Common.Exceptions;
using EscolaBiblica.Application.Common.Interfaces;
using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Domain.Interfaces;
using MediatR;

namespace EscolaBiblica.Application.Features.Igrejas.Commands;

public record CriarIgrejaCommand(string Nome, string? Descricao, string? Endereco, 
    string? Telefone, string? Email) : IRequest<Guid>;

public class CriarIgrejaCommandHandler : IRequestHandler<CriarIgrejaCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public CriarIgrejaCommandHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<Guid> Handle(CriarIgrejaCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe igreja com o mesmo nome
        var igrejaExistente = await _unitOfWork.Igrejas.ObterPorNomeAsync(request.Nome, cancellationToken);
        if (igrejaExistente != null)
            throw new ConflictException($"Já existe uma igreja com o nome '{request.Nome}'");

        var criadoPor = _usuarioAtual.Id ?? throw new ForbiddenException("Usuário não autenticado");

        var igreja = new Igreja(request.Nome, criadoPor);
        
        if (!string.IsNullOrEmpty(request.Descricao) || !string.IsNullOrEmpty(request.Endereco) ||
            !string.IsNullOrEmpty(request.Telefone) || !string.IsNullOrEmpty(request.Email))
        {
            igreja.AtualizarInformacoes(request.Nome, request.Descricao, request.Endereco, 
                request.Telefone, request.Email, criadoPor);
        }

        await _unitOfWork.Igrejas.AdicionarAsync(igreja, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return igreja.Id;
    }
}

public record AtualizarIgrejaCommand(Guid Id, string Nome, string? Descricao, string? Endereco,
    string? Telefone, string? Email) : IRequest;

public class AtualizarIgrejaCommandHandler : IRequestHandler<AtualizarIgrejaCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioAtual _usuarioAtual;

    public AtualizarIgrejaCommandHandler(IUnitOfWork unitOfWork, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _usuarioAtual = usuarioAtual;
    }

    public async Task Handle(AtualizarIgrejaCommand request, CancellationToken cancellationToken)
    {
        var igreja = await _unitOfWork.Igrejas.ObterPorIdAsync(request.Id, cancellationToken);
        if (igreja == null)
            throw new NotFoundException(nameof(Igreja), request.Id);

        if (!_usuarioAtual.PodeAcessarIgreja(igreja.Id))
            throw new ForbiddenException("Sem permissão para atualizar esta igreja");

        // Verificar se o novo nome já existe (exceto para a própria igreja)
        if (await _unitOfWork.Igrejas.NomeJaExisteAsync(request.Nome, request.Id, cancellationToken))
            throw new ConflictException($"Já existe outra igreja com o nome '{request.Nome}'");

        var atualizadoPor = _usuarioAtual.Id!.Value;
        igreja.AtualizarInformacoes(request.Nome, request.Descricao, request.Endereco,
            request.Telefone, request.Email, atualizadoPor);

        await _unitOfWork.Igrejas.AtualizarAsync(igreja, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}