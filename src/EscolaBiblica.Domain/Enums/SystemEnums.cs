namespace EscolaBiblica.Domain.Enums;

public enum TipoPerfil
{
    AdministradorGeral = 1,
    AdministradorIgreja = 2,
    Pastor = 3,
    Coordenador = 4,
    Professor = 5,
    Secretario = 6,
    Visualizador = 7
}

public enum StatusCompetitiva
{
    Ativa = 1,
    Pausada = 2,
    Finalizada = 3,
    Cancelada = 4
}

public enum TipoRegra
{
    PontuacaoIndividual = 1,
    PontuacaoTurma = 2,
    CriterioDesempate = 3,
    ConfiguracaoGeral = 4
}

public enum StatusPresenca
{
    Presente = 1,
    Ausente = 2,
    Falta = 3
}