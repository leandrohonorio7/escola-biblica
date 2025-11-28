using EscolaBiblica.Application.Features.Alunos.Commands;
using EscolaBiblica.Application.Features.Alunos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaBiblica.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlunosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AlunosController> _logger;

    public AlunosController(IMediator mediator, ILogger<AlunosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os alunos ou por turma específica
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterAlunos([FromQuery] Guid? turmaId = null)
    {
        _logger.LogInformation("Obtendo alunos{TurmaInfo}", 
            turmaId.HasValue ? $" da turma {turmaId}" : "");
        
        var alunos = await _mediator.Send(new ObterAlunosQuery(turmaId));
        return Ok(alunos);
    }

    /// <summary>
    /// Obtém um aluno específico por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterAluno(Guid id)
    {
        _logger.LogInformation("Obtendo aluno {AlunoId}", id);
        var aluno = await _mediator.Send(new ObterAlunoQuery(id));
        return Ok(aluno);
    }

    /// <summary>
    /// Obtém aniversariantes do mês
    /// </summary>
    [HttpGet("aniversariantes/{mes:int}")]
    public async Task<IActionResult> ObterAniversariantes(int mes)
    {
        if (mes < 1 || mes > 12)
            return BadRequest("Mês deve estar entre 1 e 12");

        _logger.LogInformation("Obtendo aniversariantes do mês {Mes}", mes);
        var aniversariantes = await _mediator.Send(new ObterAniversariantesQuery(mes));
        return Ok(aniversariantes);
    }

    /// <summary>
    /// Cria um novo aluno
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "AdministradorGeral,AdministradorIgreja,Pastor,Coordenador,Professor,Secretario")]
    public async Task<IActionResult> CriarAluno([FromBody] CriarAlunoCommand command)
    {
        _logger.LogInformation("Criando novo aluno: {Nome}", command.Nome);
        var alunoId = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterAluno), new { id = alunoId }, new { Id = alunoId });
    }

    /// <summary>
    /// Matricula um aluno em uma turma
    /// </summary>
    [HttpPost("{id:guid}/matricular")]
    [Authorize(Roles = "AdministradorGeral,AdministradorIgreja,Pastor,Coordenador,Professor,Secretario")]
    public async Task<IActionResult> MatricularAluno(Guid id, [FromBody] MatricularAlunoRequest request)
    {
        _logger.LogInformation("Matriculando aluno {AlunoId} na turma {TurmaId}", id, request.TurmaId);
        await _mediator.Send(new MatricularAlunoCommand(id, request.TurmaId));
        return NoContent();
    }
}

public record MatricularAlunoRequest(Guid TurmaId);