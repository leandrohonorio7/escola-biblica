namespace EscolaBiblica.BlazorWASM.Models;

public class IgrejaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
}

public class AlunoDto
{
    public Guid Id { get; set; }
    public Guid IgrejaId { get; set; }
    public Guid? TurmaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? NomePai { get; set; }
    public string? NomeMae { get; set; }
    public string? TelefonePai { get; set; }
    public string? TelefoneMae { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    
    // Propriedades de navegação
    public string? NomeIgreja { get; set; }
    public string? NomeTurma { get; set; }
    public int TotalPresencas { get; set; }
    public decimal PercentualPresenca { get; set; }
}

public class TurmaDto
{
    public Guid Id { get; set; }
    public Guid IgrejaId { get; set; }
    public Guid? ProfessorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int IdadeMinima { get; set; }
    public int IdadeMaxima { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    
    // Propriedades de navegação
    public string? NomeIgreja { get; set; }
    public string? NomeProfessor { get; set; }
    public int TotalAlunos { get; set; }
    public List<AlunoDto> Alunos { get; set; } = new();
}

public class PresencaDto
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public DateTime DataAula { get; set; }
    public bool Presente { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataRegistro { get; set; }
    public Guid UsuarioRegistroId { get; set; }
    
    // Propriedades de navegação
    public string? NomeAluno { get; set; }
    public string? NomeUsuarioRegistro { get; set; }
}

public class CompetitivaDto
{
    public Guid Id { get; set; }
    public Guid IgrejaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public bool Ativo { get; set; } = true;
    public string? ConfiguracoesJson { get; set; }
    public DateTime DataCriacao { get; set; }
    
    // Propriedades de navegação
    public string? NomeIgreja { get; set; }
    public List<RegraCompetitivaDto> Regras { get; set; } = new();
    public List<PontuacaoCompetitivaDto> Pontuacoes { get; set; } = new();
}

public class RegraCompetitivaDto
{
    public Guid Id { get; set; }
    public Guid CompetitivaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PontuacaoBase { get; set; }
    public string? CondicoesJson { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
}

public class PontuacaoCompetitivaDto
{
    public Guid Id { get; set; }
    public Guid CompetitivaId { get; set; }
    public Guid AlunoId { get; set; }
    public DateTime DataPontuacao { get; set; }
    public decimal Pontos { get; set; }
    public string? Descricao { get; set; }
    public string? DetalhesJson { get; set; }
    public Guid UsuarioRegistroId { get; set; }
    
    // Propriedades de navegação
    public string? NomeAluno { get; set; }
    public string? NomeCompetitiva { get; set; }
    public string? NomeUsuarioRegistro { get; set; }
}

public class UsuarioDto
{
    public Guid Id { get; set; }
    public Guid? IgrejaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string TipoUsuario { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    
    // Propriedades de navegação
    public string? NomeIgreja { get; set; }
    public List<string> Roles { get; set; } = new();
}

// DTOs para comandos
public class CriarIgrejaCommand
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}

public class AtualizarIgrejaCommand
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; }
}

public class CriarAlunoCommand
{
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? NomePai { get; set; }
    public string? NomeMae { get; set; }
    public string? TelefonePai { get; set; }
    public string? TelefoneMae { get; set; }
    public Guid? TurmaId { get; set; }
}

public class AtualizarAlunoCommand
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? NomePai { get; set; }
    public string? NomeMae { get; set; }
    public string? TelefonePai { get; set; }
    public string? TelefoneMae { get; set; }
    public Guid? TurmaId { get; set; }
    public bool Ativo { get; set; }
}

public class RegistrarPresencaCommand
{
    public Guid AlunoId { get; set; }
    public DateTime DataAula { get; set; }
    public bool Presente { get; set; }
    public string? Observacoes { get; set; }
}