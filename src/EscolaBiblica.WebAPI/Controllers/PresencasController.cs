using EscolaBiblica.Application.Features.Presencas.Commands;
using EscolaBiblica.Application.Features.Presencas.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaBiblica.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresencasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PresencasController> _logger;

    public PresencasController(IMediator mediator, ILogger<PresencasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém presenças de uma turma em uma data específica
    /// </summary>
    [HttpGet("turma/{turmaId:guid}")]
    public async Task<IActionResult> ObterPresencasTurma(Guid turmaId, [FromQuery] DateTime data)
    {
        _logger.LogInformation("Obtendo presenças da turma {TurmaId} na data {Data:yyyy-MM-dd}", 
            turmaId, data);
        
        var presencas = await _mediator.Send(new ObterPresencasTurmaQuery(turmaId, data));
        return Ok(presencas);
    }

    /// <summary>
    /// Obtém presenças de um aluno
    /// </summary>
    [HttpGet("aluno/{alunoId:guid}")]
    public async Task<IActionResult> ObterPresencasAluno(Guid alunoId, 
        [FromQuery] DateTime? dataInicio = null, 
        [FromQuery] DateTime? dataFim = null)
    {
        _logger.LogInformation("Obtendo presenças do aluno {AlunoId}", alunoId);
        
        var presencas = await _mediator.Send(new ObterPresencasAlunoQuery(alunoId, dataInicio, dataFim));
        return Ok(presencas);
    }

    /// <summary>
    /// Obtém relatório de presenças por período
    /// </summary>
    [HttpGet("relatorio")]
    public async Task<IActionResult> ObterRelatorioPresencas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] Guid? turmaId = null)
    {
        _logger.LogInformation("Gerando relatório de presenças de {DataInicio:yyyy-MM-dd} a {DataFim:yyyy-MM-dd}{TurmaInfo}",
            dataInicio, dataFim, turmaId.HasValue ? $" para turma {turmaId}" : "");
        
        var relatorio = await _mediator.Send(new ObterRelatorioPresencasQuery(dataInicio, dataFim, turmaId));
        return Ok(relatorio);
    }

    /// <summary>
    /// Registra presença de um aluno
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "AdministradorGeral,AdministradorIgreja,Pastor,Coordenador,Professor,Secretario")]
    public async Task<IActionResult> RegistrarPresenca([FromBody] RegistrarPresencaCommand command)
    {
        _logger.LogInformation("Registrando presença do aluno {AlunoId} na data {Data:yyyy-MM-dd}", 
            command.AlunoId, command.Data);
        
        var presencaId = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterPresencasAluno), 
            new { alunoId = command.AlunoId }, 
            new { Id = presencaId });
    }

    /// <summary>
    /// Registra presenças de uma turma inteira
    /// </summary>
    [HttpPost("turma")]
    [Authorize(Roles = "AdministradorGeral,AdministradorIgreja,Pastor,Coordenador,Professor,Secretario")]
    public async Task<IActionResult> RegistrarPresencasTurma([FromBody] RegistrarPresencasTurmaCommand command)
    {
        _logger.LogInformation("Registrando presenças da turma {TurmaId} na data {Data:yyyy-MM-dd}", 
            command.TurmaId, command.Data);
        
        await _mediator.Send(command);
        return Ok(new { Message = "Presenças registradas com sucesso" });
    }
}