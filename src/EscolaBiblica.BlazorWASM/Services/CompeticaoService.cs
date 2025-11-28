using EscolaBiblica.BlazorWASM.Models;
using System.Net.Http.Json;

namespace EscolaBiblica.BlazorWASM.Services;

public interface ICompeticaoService
{
    Task<List<CompeticaoDto>> GetCompeticoesAsync();
    Task<CompeticaoDto?> GetCompeticaoByIdAsync(Guid id);
    Task<CompeticaoDto> CreateCompeticaoAsync(CompeticaoDto competicao);
    Task<CompeticaoDto> UpdateCompeticaoAsync(CompeticaoDto competicao);
    Task<bool> DeleteCompeticaoAsync(Guid id);
    Task<List<ParticipanteCompeticaoDto>> GetParticipantesAsync(Guid competicaoId);
    Task<bool> InserirParticipanteAsync(Guid competicaoId, Guid alunoId);
    Task<bool> RemoverParticipanteAsync(Guid competicaoId, Guid alunoId);
    Task<bool> AvaliarParticipanteAsync(Guid participanteId, List<AvaliacaoCriterioDto> avaliacoes);
    Task<List<ParticipanteCompeticaoDto>> GetRankingAsync(Guid competicaoId);
    Task<bool> FinalizarCompeticaoAsync(Guid competicaoId);
}

public class CompeticaoService : ICompeticaoService
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CompeticaoService> _logger;
    private const string ApiEndpoint = "api/competicoes";
    
    // Dados simulados para demonstração
    private static List<CompeticaoDto> _competicoes = new();
    private static List<ParticipanteCompeticaoDto> _participantes = new();

    public CompeticaoService(HttpClient httpClient, ICacheService cacheService, ILogger<CompeticaoService> logger)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _logger = logger;
        
        // Inicializar dados de exemplo se estiver vazio
        if (!_competicoes.Any())
        {
            InicializarDadosExemplo();
        }
    }

    public async Task<List<CompeticaoDto>> GetCompeticoesAsync()
    {
        try
        {
            // Tentar buscar da API primeiro
            var response = await _httpClient.GetAsync(ApiEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var competicoes = await response.Content.ReadFromJsonAsync<List<CompeticaoDto>>();
                return competicoes ?? new List<CompeticaoDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar competições da API, usando dados locais");
        }

        // Fallback para dados locais
        await Task.Delay(500); // Simular latência
        return _competicoes.OrderByDescending(c => c.DataCriacao).ToList();
    }

    public async Task<CompeticaoDto?> GetCompeticaoByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CompeticaoDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar competição {Id} da API, usando dados locais", id);
        }

        await Task.Delay(300);
        return _competicoes.FirstOrDefault(c => c.Id == id);
    }

    public async Task<CompeticaoDto> CreateCompeticaoAsync(CompeticaoDto competicao)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, competicao);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<CompeticaoDto>();
                if (created != null) return created;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao criar competição na API, salvando localmente");
        }

        // Fallback local
        competicao.Id = Guid.NewGuid();
        competicao.DataCriacao = DateTime.Now;
        competicao.Status = StatusCompeticao.Planejada;
        
        _competicoes.Add(competicao);
        
        await Task.Delay(300);
        return competicao;
    }

    public async Task<CompeticaoDto> UpdateCompeticaoAsync(CompeticaoDto competicao)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{competicao.Id}", competicao);
            if (response.IsSuccessStatusCode)
            {
                var updated = await response.Content.ReadFromJsonAsync<CompeticaoDto>();
                if (updated != null) return updated;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar competição na API, atualizando localmente");
        }

        var existing = _competicoes.FirstOrDefault(c => c.Id == competicao.Id);
        if (existing != null)
        {
            var index = _competicoes.IndexOf(existing);
            competicao.DataUltimaAlteracao = DateTime.Now;
            _competicoes[index] = competicao;
        }

        await Task.Delay(300);
        return competicao;
    }

    public async Task<bool> DeleteCompeticaoAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao excluir competição da API, removendo localmente");
        }

        var competicao = _competicoes.FirstOrDefault(c => c.Id == id);
        if (competicao != null)
        {
            _competicoes.Remove(competicao);
            await Task.Delay(200);
            return true;
        }

        return false;
    }

    public async Task<List<ParticipanteCompeticaoDto>> GetParticipantesAsync(Guid competicaoId)
    {
        await Task.Delay(300);
        return _participantes.Where(p => p.CompeticaoId == competicaoId)
                           .OrderBy(p => p.Posicao)
                           .ThenByDescending(p => p.NotaFinal)
                           .ToList();
    }

    public async Task<bool> InserirParticipanteAsync(Guid competicaoId, Guid alunoId)
    {
        await Task.Delay(200);
        
        var jaExiste = _participantes.Any(p => p.CompeticaoId == competicaoId && p.AlunoId == alunoId);
        if (jaExiste) return false;

        var participante = new ParticipanteCompeticaoDto
        {
            Id = Guid.NewGuid(),
            CompeticaoId = competicaoId,
            AlunoId = alunoId,
            NomeAluno = $"Aluno {alunoId.ToString()[..8]}",
            NomeTurma = "Turma Exemplo",
            DataInscricao = DateTime.Now,
            Status = StatusParticipacao.Inscrito
        };

        _participantes.Add(participante);
        return true;
    }

    public async Task<bool> RemoverParticipanteAsync(Guid competicaoId, Guid alunoId)
    {
        await Task.Delay(200);
        
        var participante = _participantes.FirstOrDefault(p => p.CompeticaoId == competicaoId && p.AlunoId == alunoId);
        if (participante != null)
        {
            _participantes.Remove(participante);
            return true;
        }

        return false;
    }

    public async Task<bool> AvaliarParticipanteAsync(Guid participanteId, List<AvaliacaoCriterioDto> avaliacoes)
    {
        await Task.Delay(300);
        
        var participante = _participantes.FirstOrDefault(p => p.Id == participanteId);
        if (participante == null) return false;

        participante.Avaliacoes = avaliacoes;
        
        // Calcular notas
        participante.NotaComportamento = avaliacoes.Where(a => a.NomeCriterio.Contains("Comportamento")).Average(a => a.Nota);
        participante.NotaProva = avaliacoes.Where(a => a.NomeCriterio.Contains("Conhecimento")).Average(a => a.Nota);
        
        // Calcular nota final (exemplo simplificado)
        participante.NotaFinal = (participante.NotaProva * 0.4m) + 
                                (participante.NotaPresenca * 0.3m) + 
                                (participante.NotaComportamento * 0.3m);

        return true;
    }

    public async Task<List<ParticipanteCompeticaoDto>> GetRankingAsync(Guid competicaoId)
    {
        await Task.Delay(300);
        
        var participantes = _participantes.Where(p => p.CompeticaoId == competicaoId)
                                         .OrderByDescending(p => p.NotaFinal)
                                         .ToList();

        for (int i = 0; i < participantes.Count; i++)
        {
            participantes[i].Posicao = i + 1;
        }

        return participantes;
    }

    public async Task<bool> FinalizarCompeticaoAsync(Guid competicaoId)
    {
        await Task.Delay(200);
        
        var competicao = _competicoes.FirstOrDefault(c => c.Id == competicaoId);
        if (competicao != null)
        {
            competicao.Status = StatusCompeticao.Finalizada;
            competicao.DataUltimaAlteracao = DateTime.Now;
            return true;
        }

        return false;
    }

    private void InicializarDadosExemplo()
    {
        var competicao1 = new CompeticaoDto
        {
            Id = Guid.NewGuid(),
            Nome = "Concurso Bíblico 2024",
            Descricao = "Competição anual de conhecimentos bíblicos para todas as turmas",
            DataInicio = DateTime.Now.AddDays(-30),
            DataFim = DateTime.Now.AddDays(30),
            Tipo = TipoCompeticao.Individual,
            Status = StatusCompeticao.EmAndamento,
            PesoProva = 50,
            PesoPresenca = 25,
            PesoComportamento = 25,
            DataCriacao = DateTime.Now.AddDays(-35),
            UsuarioCriacao = "Admin"
        };

        var competicao2 = new CompeticaoDto
        {
            Id = Guid.NewGuid(),
            Nome = "Gincana das Turmas",
            Descricao = "Competição por equipes com atividades diversas",
            DataInicio = DateTime.Now.AddDays(15),
            DataFim = DateTime.Now.AddDays(45),
            Tipo = TipoCompeticao.PorTurma,
            Status = StatusCompeticao.Inscricoes,
            PesoProva = 40,
            PesoPresenca = 30,
            PesoComportamento = 30,
            DataCriacao = DateTime.Now.AddDays(-10),
            UsuarioCriacao = "Admin"
        };

        _competicoes.AddRange(new[] { competicao1, competicao2 });
    }
}