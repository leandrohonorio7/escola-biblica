namespace EscolaBiblica.Domain.Common;

public abstract class EntidadeBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public Guid IgrejaId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public Guid CriadoPor { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public Guid? AtualizadoPor { get; set; }

    protected EntidadeBase() { }

    protected EntidadeBase(Guid igrejaId, Guid criadoPor)
    {
        IgrejaId = igrejaId;
        CriadoPor = criadoPor;
    }

    public void Atualizar(Guid atualizadoPor)
    {
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPor = atualizadoPor;
    }
}