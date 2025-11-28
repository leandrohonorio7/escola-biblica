using EscolaBiblica.Domain.Common;
using EscolaBiblica.Domain.Enums;

namespace EscolaBiblica.Domain.Entities;

public class Usuario : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public DateTime? DataNascimento { get; private set; }
    public TipoPerfil Perfil { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime? UltimoAcesso { get; private set; }

    // Identity Integration
    public string IdentityUserId { get; private set; } = string.Empty;

    // Relacionamentos
    public virtual Igreja Igreja { get; private set; } = null!;
    public virtual ICollection<Turma> TurmasComoProfessor { get; private set; } = [];

    protected Usuario() { }

    public Usuario(string nome, string email, string identityUserId, TipoPerfil perfil, 
        Guid igrejaId, Guid criadoPor) : base(igrejaId, criadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        IdentityUserId = identityUserId ?? throw new ArgumentNullException(nameof(identityUserId));
        Perfil = perfil;
    }

    public void AtualizarInformacoes(string nome, string? telefone, DateTime? dataNascimento, Guid atualizadoPor)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Telefone = telefone;
        DataNascimento = dataNascimento;
        Atualizar(atualizadoPor);
    }

    public void AlterarPerfil(TipoPerfil novoPerfil, Guid atualizadoPor)
    {
        Perfil = novoPerfil;
        Atualizar(atualizadoPor);
    }

    public void RegistrarUltimoAcesso()
    {
        UltimoAcesso = DateTime.UtcNow;
    }

    public void Ativar(Guid atualizadoPor)
    {
        Ativo = true;
        Atualizar(atualizadoPor);
    }

    public void Desativar(Guid atualizadoPor)
    {
        Ativo = false;
        Atualizar(atualizadoPor);
    }

    public bool PodeAcessarIgreja(Guid igrejaId)
    {
        if (Perfil == TipoPerfil.AdministradorGeral)
            return true;
        
        return IgrejaId == igrejaId;
    }

    public bool PodeGerenciarTurmas() =>
        Perfil is TipoPerfil.AdministradorGeral 
                or TipoPerfil.AdministradorIgreja 
                or TipoPerfil.Pastor 
                or TipoPerfil.Coordenador 
                or TipoPerfil.Professor;

    public bool PodeGerenciarUsuarios() =>
        Perfil is TipoPerfil.AdministradorGeral 
                or TipoPerfil.AdministradorIgreja;

    public bool PodeGerenciarCompetitivas() =>
        Perfil is TipoPerfil.AdministradorGeral 
                or TipoPerfil.AdministradorIgreja 
                or TipoPerfil.Pastor 
                or TipoPerfil.Coordenador;
}