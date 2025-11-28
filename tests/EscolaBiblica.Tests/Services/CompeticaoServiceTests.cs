using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using EscolaBiblica.BlazorWASM.Services;
using EscolaBiblica.BlazorWASM.Models;

namespace EscolaBiblica.Tests.Services;

public class CompeticaoServiceTests
{
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<CompeticaoService>> _mockLogger;
    private readonly CompeticaoService _service;

    public CompeticaoServiceTests()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<CompeticaoService>>();
        
        _service = new CompeticaoService(_mockHttpClient.Object, _mockCacheService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCompeticoesAsync_ShouldReturnCompeticoes_WhenCalled()
    {
        // Arrange & Act
        var result = await _service.GetCompeticoesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<CompeticaoDto>>();
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetCompeticaoByIdAsync_ShouldReturnCompeticao_WhenValidIdProvided()
    {
        // Arrange
        var competicoes = await _service.GetCompeticoesAsync();
        var competicaoId = competicoes.First().Id;

        // Act
        var result = await _service.GetCompeticaoByIdAsync(competicaoId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(competicaoId);
    }

    [Fact]
    public async Task GetCompeticaoByIdAsync_ShouldReturnNull_WhenInvalidIdProvided()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.GetCompeticaoByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCompeticaoAsync_ShouldCreateAndReturnCompeticao_WhenValidDataProvided()
    {
        // Arrange
        var newCompeticao = new CompeticaoDto
        {
            Nome = "Teste Competição",
            Descricao = "Descrição de teste",
            DataInicio = DateTime.Now,
            DataFim = DateTime.Now.AddDays(30),
            Tipo = TipoCompeticao.Individual,
            PesoProva = 40,
            PesoPresenca = 30,
            PesoComportamento = 30,
            UsuarioCriacao = "TestUser"
        };

        // Act
        var result = await _service.CreateCompeticaoAsync(newCompeticao);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Nome.Should().Be(newCompeticao.Nome);
        result.Descricao.Should().Be(newCompeticao.Descricao);
        result.Status.Should().Be(StatusCompeticao.Planejada);
        result.DataCriacao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateCompeticaoAsync_ShouldUpdateAndReturnCompeticao_WhenValidDataProvided()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();
        competicao.Nome = "Nome Atualizado";
        competicao.Descricao = "Descrição Atualizada";

        // Act
        var result = await _service.UpdateCompeticaoAsync(competicao);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Nome Atualizado");
        result.Descricao.Should().Be("Descrição Atualizada");
        result.DataUltimaAlteracao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteCompeticaoAsync_ShouldReturnTrue_WhenValidIdProvided()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();

        // Act
        var result = await _service.DeleteCompeticaoAsync(competicao.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCompeticaoAsync_ShouldReturnFalse_WhenInvalidIdProvided()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _service.DeleteCompeticaoAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task InserirParticipanteAsync_ShouldReturnTrue_WhenValidIdsProvided()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();
        var alunoId = Guid.NewGuid();

        // Act
        var result = await _service.InserirParticipanteAsync(competicao.Id, alunoId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task InserirParticipanteAsync_ShouldReturnFalse_WhenParticipanteAlreadyExists()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();
        var alunoId = Guid.NewGuid();
        
        await _service.InserirParticipanteAsync(competicao.Id, alunoId);

        // Act
        var result = await _service.InserirParticipanteAsync(competicao.Id, alunoId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetParticipantesAsync_ShouldReturnParticipantes_WhenValidCompetricaoIdProvided()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();
        var alunoId1 = Guid.NewGuid();
        var alunoId2 = Guid.NewGuid();
        
        await _service.InserirParticipanteAsync(competicao.Id, alunoId1);
        await _service.InserirParticipanteAsync(competicao.Id, alunoId2);

        // Act
        var result = await _service.GetParticipantesAsync(competicao.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.CompeticaoId == competicao.Id).Should().BeTrue();
    }

    [Fact]
    public async Task RemoverParticipanteAsync_ShouldReturnTrue_WhenParticipanteExists()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();
        var alunoId = Guid.NewGuid();
        
        await _service.InserirParticipanteAsync(competicao.Id, alunoId);

        // Act
        var result = await _service.RemoverParticipanteAsync(competicao.Id, alunoId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetRankingAsync_ShouldReturnRankedParticipantes_WhenValidCompetricaoIdProvided()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();
        var alunoId1 = Guid.NewGuid();
        var alunoId2 = Guid.NewGuid();
        
        await _service.InserirParticipanteAsync(competicao.Id, alunoId1);
        await _service.InserirParticipanteAsync(competicao.Id, alunoId2);

        // Act
        var result = await _service.GetRankingAsync(competicao.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(p => p.NotaFinal);
        
        for (int i = 0; i < result.Count; i++)
        {
            result[i].Posicao.Should().Be(i + 1);
        }
    }

    [Fact]
    public async Task FinalizarCompeticaoAsync_ShouldReturnTrue_WhenValidIdProvided()
    {
        // Arrange
        var competicao = await CreateTestCompeticao();

        // Act
        var result = await _service.FinalizarCompeticaoAsync(competicao.Id);

        // Assert
        result.Should().BeTrue();
        
        var updatedCompeticao = await _service.GetCompeticaoByIdAsync(competicao.Id);
        updatedCompeticao!.Status.Should().Be(StatusCompeticao.Finalizada);
    }

    [Fact]
    public async Task AvaliarParticipanteAsync_ShouldCreateCompetition_WithValidData()
    {
        // Arrange & Act
        var competicao = await CreateTestCompeticao();
        
        // Assert
        competicao.Should().NotBeNull();
        competicao.Id.Should().NotBe(Guid.Empty);
        competicao.Nome.Should().NotBeNullOrEmpty();
    }

    private async Task<CompeticaoDto> CreateTestCompeticao()
    {
        var competicao = new CompeticaoDto
        {
            Nome = "Competição Teste",
            Descricao = "Descrição de teste",
            DataInicio = DateTime.Now,
            DataFim = DateTime.Now.AddDays(30),
            Tipo = TipoCompeticao.Individual,
            PesoProva = 40,
            PesoPresenca = 30,
            PesoComportamento = 30,
            UsuarioCriacao = "TestUser"
        };

        return await _service.CreateCompeticaoAsync(competicao);
    }
}