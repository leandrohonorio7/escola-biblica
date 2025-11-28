namespace EscolaBiblica.Application.Common.Interfaces;

public interface IUsuarioAtual
{
    string? UserId { get; }
    string? Id { get; }
    string? Username { get; }
    string? Email { get; }
    Guid? IgrejaId { get; }
    bool IsAuthenticated { get; }
    bool EhAdministradorGeral { get; }
    Task<bool> IsInRoleAsync(string role);
    Task<string[]> GetRolesAsync();
    Task<bool> PodeAcessarIgreja(Guid igrejaId);
}