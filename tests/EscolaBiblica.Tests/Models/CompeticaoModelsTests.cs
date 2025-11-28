using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using EscolaBiblica.BlazorWASM.Models;

namespace EscolaBiblica.Tests.Models;

public class CompeticaoDtoTests
{
    [Fact]
    public void CompeticaoDto_ShouldHaveValidDefaultValues_WhenInstantiated()
    {
        // Act
        var competicao = new CompeticaoDto();

        // Assert
        competicao.Id.Should().Be(Guid.Empty);
        competicao.Nome.Should().Be(string.Empty);
        competicao.Descricao.Should().Be(string.Empty);
        competicao.PesoProva.Should().Be(40);
        competicao.PesoPresenca.Should().Be(30);
        competicao.PesoComportamento.Should().Be(30);
        competicao.Criterios.Should().NotBeNull();
        competicao.Criterios.Should().BeEmpty();
        competicao.Participantes.Should().NotBeNull();
        competicao.Participantes.Should().BeEmpty();
        competicao.UsuarioCriacao.Should().Be(string.Empty);
    }

    [Fact]
    public void CompeticaoDto_ShouldFailValidation_WhenNomeIsEmpty()
    {
        // Arrange
        var competicao = new CompeticaoDto
        {
            Nome = "",
            Descricao = "Descrição válida"
        };

        // Act
        var validationResults = ValidateModel(competicao);

        // Assert
        validationResults.Should().Contain(vr => vr.ErrorMessage == "Nome é obrigatório");
    }

    [Fact]
    public void CompeticaoDto_ShouldFailValidation_WhenNomeExceedsMaxLength()
    {
        // Arrange
        var competicao = new CompeticaoDto
        {
            Nome = new string('A', 101), // 101 caracteres
            Descricao = "Descrição válida"
        };

        // Act
        var validationResults = ValidateModel(competicao);

        // Assert
        validationResults.Should().Contain(vr => vr.ErrorMessage == "Nome deve ter no máximo 100 caracteres");
    }

    [Fact]
    public void CompeticaoDto_ShouldFailValidation_WhenDescricaoIsEmpty()
    {
        // Arrange
        var competicao = new CompeticaoDto
        {
            Nome = "Nome válido",
            Descricao = ""
        };

        // Act
        var validationResults = ValidateModel(competicao);

        // Assert
        validationResults.Should().Contain(vr => vr.ErrorMessage == "Descrição é obrigatória");
    }

    [Fact]
    public void CompeticaoDto_ShouldFailValidation_WhenPesoExceedsRange()
    {
        // Arrange
        var competicao = new CompeticaoDto
        {
            Nome = "Nome válido",
            Descricao = "Descrição válida",
            PesoProva = 101 // Acima do limite de 100
        };

        // Act
        var validationResults = ValidateModel(competicao);

        // Assert
        validationResults.Should().Contain(vr => vr.ErrorMessage == "Peso deve estar entre 0 e 100");
    }

    [Fact]
    public void CompeticaoDto_ShouldPassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var competicao = new CompeticaoDto
        {
            Nome = "Competição Válida",
            Descricao = "Descrição válida",
            Tipo = TipoCompeticao.Individual,
            PesoProva = 40,
            PesoPresenca = 30,
            PesoComportamento = 30
        };

        // Act
        var validationResults = ValidateModel(competicao);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 50, 50)]
    [InlineData(100, 0, 0)]
    [InlineData(33, 33, 34)]
    public void CompeticaoDto_ShouldAcceptValidPesoDistributions(decimal pesoProva, decimal pesoPresenca, decimal pesoComportamento)
    {
        // Arrange
        var competicao = new CompeticaoDto
        {
            Nome = "Teste",
            Descricao = "Teste",
            PesoProva = pesoProva,
            PesoPresenca = pesoPresenca,
            PesoComportamento = pesoComportamento
        };

        // Act
        var validationResults = ValidateModel(competicao);

        // Assert
        validationResults.Where(vr => vr.ErrorMessage!.Contains("Peso deve estar entre 0 e 100"))
                        .Should().BeEmpty();
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}

public class CriterioAvaliacaoDtoTests
{
    [Fact]
    public void CriterioAvaliacaoDto_ShouldHaveValidDefaultValues_WhenInstantiated()
    {
        // Act
        var criterio = new CriterioAvaliacaoDto();

        // Assert
        criterio.Id.Should().Be(Guid.Empty);
        criterio.CompeticaoId.Should().Be(Guid.Empty);
        criterio.Nome.Should().Be(string.Empty);
        criterio.Descricao.Should().Be(string.Empty);
        criterio.Peso.Should().Be(0);
        criterio.NotaMaxima.Should().Be(10);
        criterio.Ativo.Should().BeTrue();
    }

    [Fact]
    public void CriterioAvaliacaoDto_ShouldFailValidation_WhenNomeIsEmpty()
    {
        // Arrange
        var criterio = new CriterioAvaliacaoDto
        {
            Nome = "",
            Peso = 50,
            NotaMaxima = 10
        };

        // Act
        var validationResults = ValidateModel(criterio);

        // Assert
        validationResults.Should().Contain(vr => vr.ErrorMessage == "Nome do critério é obrigatório");
    }

    [Fact]
    public void CriterioAvaliacaoDto_ShouldPassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var criterio = new CriterioAvaliacaoDto
        {
            Nome = "Conhecimento Bíblico",
            Descricao = "Avaliação do conhecimento das escrituras",
            Peso = 50,
            NotaMaxima = 10,
            TipoCriterio = TipoCriterio.Conhecimento
        };

        // Act
        var validationResults = ValidateModel(criterio);

        // Assert
        validationResults.Should().BeEmpty();
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}

public class ParticipanteCompeticaoDtoTests
{
    [Fact]
    public void ParticipanteCompeticaoDto_ShouldHaveValidDefaultValues_WhenInstantiated()
    {
        // Act
        var participante = new ParticipanteCompeticaoDto();

        // Assert
        participante.Id.Should().Be(Guid.Empty);
        participante.CompeticaoId.Should().Be(Guid.Empty);
        participante.AlunoId.Should().Be(Guid.Empty);
        participante.NomeAluno.Should().Be(string.Empty);
        participante.NomeTurma.Should().Be(string.Empty);
        participante.NotaProva.Should().Be(0);
        participante.NotaPresenca.Should().Be(0);
        participante.NotaComportamento.Should().Be(0);
        participante.NotaFinal.Should().Be(0);
        participante.Posicao.Should().Be(0);
        participante.Status.Should().Be(StatusParticipacao.Inscrito);
        participante.Avaliacoes.Should().NotBeNull();
        participante.Avaliacoes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(8.5, 9.0, 7.5)]
    [InlineData(10.0, 10.0, 10.0)]
    [InlineData(0.0, 5.0, 2.5)]
    public void ParticipanteCompeticaoDto_ShouldAcceptValidNotes(decimal notaProva, decimal notaPresenca, decimal notaComportamento)
    {
        // Act
        var participante = new ParticipanteCompeticaoDto
        {
            NotaProva = notaProva,
            NotaPresenca = notaPresenca,
            NotaComportamento = notaComportamento
        };

        // Assert
        participante.NotaProva.Should().Be(notaProva);
        participante.NotaPresenca.Should().Be(notaPresenca);
        participante.NotaComportamento.Should().Be(notaComportamento);
    }
}

public class AvaliacaoCriterioDtoTests
{
    [Fact]
    public void AvaliacaoCriterioDto_ShouldHaveValidDefaultValues_WhenInstantiated()
    {
        // Act
        var avaliacao = new AvaliacaoCriterioDto();

        // Assert
        avaliacao.Id.Should().Be(Guid.Empty);
        avaliacao.ParticipanteId.Should().Be(Guid.Empty);
        avaliacao.CriterioId.Should().Be(Guid.Empty);
        avaliacao.NomeCriterio.Should().Be(string.Empty);
        avaliacao.Nota.Should().Be(0);
        avaliacao.Observacoes.Should().Be(string.Empty);
        avaliacao.AvaliadorNome.Should().Be(string.Empty);
    }

    [Fact]
    public void AvaliacaoCriterioDto_ShouldFailValidation_WhenNotaExceedsRange()
    {
        // Arrange
        var avaliacao = new AvaliacaoCriterioDto
        {
            Nota = 11, // Acima do limite de 10
            Observacoes = "Teste"
        };

        // Act
        var validationResults = ValidateModel(avaliacao);

        // Assert
        validationResults.Should().Contain(vr => vr.ErrorMessage == "Nota deve estar entre 0 e 10");
    }

    [Fact]
    public void AvaliacaoCriterioDto_ShouldPassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var avaliacao = new AvaliacaoCriterioDto
        {
            NomeCriterio = "Conhecimento",
            Nota = 8.5m,
            Observacoes = "Boa performance",
            AvaliadorNome = "Professor Silva"
        };

        // Act
        var validationResults = ValidateModel(avaliacao);

        // Assert
        validationResults.Should().BeEmpty();
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}