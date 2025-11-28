using AutoMapper;
using EscolaBiblica.Application.Common.Exceptions;
using EscolaBiblica.Application.Common.Mappings;
using EscolaBiblica.Domain.Interfaces;
using EscolaBiblica.Infrastructure.Persistence;
using MediatR;

namespace EscolaBiblica.Application.Features.Igrejas.Queries;

public record ObterIgrejaQuery(Guid Id) : IRequest<IgrejaDto>;

public class ObterIgrejaQueryHandler : IRequestHandler<ObterIgrejaQuery, IgrejaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterIgrejaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<IgrejaDto> Handle(ObterIgrejaQuery request, CancellationToken cancellationToken)
    {
        var igreja = await _unitOfWork.Igrejas.ObterPorIdAsync(request.Id, cancellationToken);
        if (igreja == null)
            throw new NotFoundException("Igreja", request.Id);

        if (!_usuarioAtual.PodeAcessarIgreja(igreja.Id))
            throw new ForbiddenException("Sem permissão para acessar esta igreja");

        return _mapper.Map<IgrejaDto>(igreja);
    }
}

public record ObterIgrejasAtivasQuery : IRequest<IEnumerable<IgrejaDto>>;

public class ObterIgrejasAtivasQueryHandler : IRequestHandler<ObterIgrejasAtivasQuery, IEnumerable<IgrejaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioAtual _usuarioAtual;

    public ObterIgrejasAtivasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioAtual usuarioAtual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<IEnumerable<IgrejaDto>> Handle(ObterIgrejasAtivasQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Igreja> igrejas;

        if (_usuarioAtual.EhAdministradorGeral())
        {
            // Administrador geral vê todas as igrejas
            igrejas = await _unitOfWork.Igrejas.ObterAtivasAsync(cancellationToken);
        }
        else if (_usuarioAtual.IgrejaId.HasValue)
        {
            // Usuário normal vê apenas sua igreja
            var igreja = await _unitOfWork.Igrejas.ObterPorIdAsync(_usuarioAtual.IgrejaId.Value, cancellationToken);
            igrejas = igreja != null && igreja.Ativa ? [igreja] : [];
        }
        else
        {
            igrejas = [];
        }

        return _mapper.Map<IEnumerable<IgrejaDto>>(igrejas);
    }
}