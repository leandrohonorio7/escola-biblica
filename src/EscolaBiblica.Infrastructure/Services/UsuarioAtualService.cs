using EscolaBiblica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EscolaBiblica.Infrastructure.Services;

public class UsuarioAtualService : IUsuarioAtual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtualService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public Guid? IgrejaId
    {
        get
        {
            var igrejaIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("IgrejaId")?.Value;
            return Guid.TryParse(igrejaIdClaim, out var igrejaId) ? igrejaId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Nome => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public IEnumerable<string> Perfis => 
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool EhAutenticado => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool EhAdministradorGeral()
    {
        return Perfis.Contains("AdministradorGeral");
    }

    public bool PodeAcessarIgreja(Guid igrejaId)
    {
        if (EhAdministradorGeral())
            return true;

        return IgrejaId == igrejaId;
    }
}