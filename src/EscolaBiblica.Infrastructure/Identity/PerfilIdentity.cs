using Microsoft.AspNetCore.Identity;

namespace EscolaBiblica.Infrastructure.Identity;

public class PerfilIdentity : IdentityRole<Guid>
{
    public string? Descricao { get; set; }
    public Guid? IgrejaId { get; set; } // null = perfil global
    public bool PodeGerenciarIgrejas { get; set; } = false;
    public string? PermissoesJson { get; set; } // JSON com permissões específicas

    public PerfilIdentity() : base()
    {
        Id = Guid.NewGuid();
    }

    public PerfilIdentity(string roleName, string? descricao = null) : base(roleName)
    {
        Id = Guid.NewGuid();
        Descricao = descricao;
        NormalizedName = roleName.ToUpperInvariant();
    }
}