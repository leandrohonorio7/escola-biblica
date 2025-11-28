using System.ComponentModel.DataAnnotations;

namespace EscolaBiblica.BlazorWASM.Models;

public class CompeticaoDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string Descricao { get; set; } = string.Empty;
    
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    
    [Required(ErrorMessage = "Tipo de competição é obrigatório")]
    public TipoCompeticao Tipo { get; set; }
    
    public StatusCompeticao Status { get; set; }
    
    [Range(0, 100, ErrorMessage = "Peso deve estar entre 0 e 100")]
    public decimal PesoProva { get; set; } = 40;
    
    [Range(0, 100, ErrorMessage = "Peso deve estar entre 0 e 100")]
    public decimal PesoPresenca { get; set; } = 30;
    
    [Range(0, 100, ErrorMessage = "Peso deve estar entre 0 e 100")]
    public decimal PesoComportamento { get; set; } = 30;
    
    public List<CriterioAvaliacaoDto> Criterios { get; set; } = new();
    public List<ParticipanteCompeticaoDto> Participantes { get; set; } = new();
    
    public DateTime DataCriacao { get; set; }
    public DateTime? DataUltimaAlteracao { get; set; }
    public string UsuarioCriacao { get; set; } = string.Empty;
}

public class CriterioAvaliacaoDto
{
    public Guid Id { get; set; }
    public Guid CompeticaoId { get; set; }
    
    [Required(ErrorMessage = "Nome do critério é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    [StringLength(300, ErrorMessage = "Descrição deve ter no máximo 300 caracteres")]
    public string Descricao { get; set; } = string.Empty;
    
    [Range(0, 100, ErrorMessage = "Peso deve estar entre 0 e 100")]
    public decimal Peso { get; set; }
    
    [Range(0, 10, ErrorMessage = "Nota máxima deve estar entre 0 e 10")]
    public decimal NotaMaxima { get; set; } = 10;
    
    public TipoCriterio TipoCriterio { get; set; }
    public bool Ativo { get; set; } = true;
}

public class ParticipanteCompeticaoDto
{
    public Guid Id { get; set; }
    public Guid CompeticaoId { get; set; }
    public Guid AlunoId { get; set; }
    public string NomeAluno { get; set; } = string.Empty;
    public string NomeTurma { get; set; } = string.Empty;
    
    public decimal NotaProva { get; set; }
    public decimal NotaPresenca { get; set; }
    public decimal NotaComportamento { get; set; }
    public decimal NotaFinal { get; set; }
    
    public int Posicao { get; set; }
    public DateTime DataInscricao { get; set; }
    public StatusParticipacao Status { get; set; } = StatusParticipacao.Inscrito;
    
    public List<AvaliacaoCriterioDto> Avaliacoes { get; set; } = new();
}

public class AvaliacaoCriterioDto
{
    public Guid Id { get; set; }
    public Guid ParticipanteId { get; set; }
    public Guid CriterioId { get; set; }
    public string NomeCriterio { get; set; } = string.Empty;
    
    [Range(0, 10, ErrorMessage = "Nota deve estar entre 0 e 10")]
    public decimal Nota { get; set; }
    
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string Observacoes { get; set; } = string.Empty;
    
    public DateTime DataAvaliacao { get; set; }
    public string AvaliadorNome { get; set; } = string.Empty;
}

public enum TipoCompeticao
{
    Individual = 1,
    Equipe = 2,
    PorTurma = 3,
    Geral = 4
}

public enum StatusCompeticao
{
    Planejada = 1,
    Inscricoes = 2,
    EmAndamento = 3,
    Avaliacao = 4,
    Finalizada = 5,
    Cancelada = 6
}

public enum TipoCriterio
{
    Conhecimento = 1,
    Comportamento = 2,
    Participacao = 3,
    Criatividade = 4,
    Lideranca = 5,
    Cooperacao = 6
}

public enum StatusParticipacao
{
    Inscrito = 1,
    Confirmado = 2,
    Participando = 3,
    Finalizado = 4,
    Desclassificado = 5,
    Desistente = 6
}