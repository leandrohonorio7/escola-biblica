using AutoMapper;
using EscolaBiblica.Domain.Entities;
using EscolaBiblica.Application.Features.Igrejas.Commands;
using EscolaBiblica.Application.Features.Alunos.Commands;
using EscolaBiblica.Application.Features.Presencas.Commands;

namespace EscolaBiblica.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Igreja
        CreateMap<Igreja, IgrejaDto>();
        CreateMap<CriarIgrejaCommand, Igreja>();

        // Usuario
        CreateMap<Usuario, UsuarioDto>();

        // Turma
        CreateMap<Turma, TurmaDto>();

        // Aluno
        CreateMap<Aluno, AlunoDto>();
        CreateMap<CriarAlunoCommand, Aluno>();

        // Presenca
        CreateMap<Presenca, PresencaDto>();
        CreateMap<RegistrarPresencaCommand, Presenca>();

        // Competitiva
        CreateMap<Competitiva, CompetitivaDto>();

        // PontuacaoCompetitiva
        CreateMap<PontuacaoCompetitiva, PontuacaoCompetitivaDto>();
    }
}

// DTOs
public record IgrejaDto(Guid Id, string Nome, string? Descricao, bool Ativa);

public record UsuarioDto(Guid Id, string Nome, string Email, string Perfil, bool Ativo);

public record TurmaDto(Guid Id, string Nome, int IdadeMinima, int IdadeMaxima, 
    TimeSpan HorarioInicio, TimeSpan HorarioFim, string? ProfessorNome, bool Ativa);

public record AlunoDto(Guid Id, string Nome, DateTime DataNascimento, 
    string? TurmaNome, bool Ativo, int Idade);

public record PresencaDto(Guid Id, DateTime Data, string StatusPresenca, 
    bool TrouxeAmigo, string AlunoNome, string TurmaNome);

public record CompetitivaDto(Guid Id, string Nome, DateTime DataInicio, 
    DateTime DataFim, string Status);

public record PontuacaoCompetitivaDto(Guid Id, DateTime Periodo, int PontosIndividuais,
    int PontosTurma, int PontosExtras, int TotalPontos, string? AlunoNome, string? TurmaNome);