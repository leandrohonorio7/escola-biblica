using EscolaBiblica.Application.Features.Igrejas.Commands;
using EscolaBiblica.Application.Features.Igrejas.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaBiblica.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IgrejasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IgrejasController> _logger;

    public IgrejasController(IMediator mediator, ILogger<IgrejasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as igrejas ativas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterIgrejasAtivas()
    {
        _logger.LogInformation("Obtendo igrejas ativas");
        var igrejas = await _mediator.Send(new ObterIgrejasAtivasQuery());
        return Ok(igrejas);
    }

    /// <summary>
    /// Obtém uma igreja específica por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterIgreja(Guid id)
    {
        _logger.LogInformation("Obtendo igreja {IgrejaId}", id);
        var igreja = await _mediator.Send(new ObterIgrejaQuery(id));
        return Ok(igreja);
    }

    /// <summary>
    /// Cria uma nova igreja
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "AdministradorGeral")]
    public async Task<IActionResult> CriarIgreja([FromBody] CriarIgrejaCommand command)
    {
        _logger.LogInformation("Criando nova igreja: {Nome}", command.Nome);
        var igrejaId = await _mediator.Send(command);
        return CreatedAtAction(nameof(ObterIgreja), new { id = igrejaId }, new { Id = igrejaId });
    }

    /// <summary>
    /// Atualiza uma igreja existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "AdministradorGeral,AdministradorIgreja")]
    public async Task<IActionResult> AtualizarIgreja(Guid id, [FromBody] AtualizarIgrejaCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID da URL não confere com ID do comando");
        }

        _logger.LogInformation("Atualizando igreja {IgrejaId}", id);
        await _mediator.Send(command);
        return NoContent();
    }
}