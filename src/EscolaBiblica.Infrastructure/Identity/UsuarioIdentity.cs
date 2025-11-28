using Microsoft.AspNetCore.Identity;

namespace EscolaBiblica.Infrastructure.Identity;

public class UsuarioIdentity : IdentityUser<Guid>
{
    public string NomeCompleto { get; set; } = string.Empty;
    public Guid? IgrejaId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime? UltimoAcesso { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    
    public UsuarioIdentity() : base()
    {
        Id = Guid.NewGuid();
    }

    public UsuarioIdentity(string userName, string email, string nomeCompleto) : base(userName)
    {
        Id = Guid.NewGuid();
        Email = email;
        NomeCompleto = nomeCompleto;
        EmailConfirmed = true; // Para simplificar inicialmente
    }
}