using Microsoft.AspNetCore.Mvc;
using MotoModel;
using MotoBusiness;
using MotoBusiness.Exceptions;
using MotoModel.Entities;
using Swashbuckle.AspNetCore.Annotations;
using MotoApi.Messaging;

namespace MotoController;

[ApiController]
[Route("api/[controller]")]
public class MotoController : ControllerBase
{
    private readonly MotoService motoService;

    public MotoController(MotoService motoService)
    {
        this.motoService = motoService;
    }

    // GET com paginação
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista motos paginadas",
        Description = "Retorna uma lista de motos paginadas. Aceita parâmetros pageNumber e pageSize.",
        OperationId = "GetPagedMotos",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = motoService.ListarTodas(pageNumber, pageSize);

        if (!result.Items.Any())
            return NoContent();

        var motosComLinks = result.Items.Select(m => new
        {
            m.id,
            m.tipoMoto,
            m.placa,
            m.numChassi,
            _links = new
            {
                self = Url.Action(nameof(Get), new { id = m.id }),
                update = Url.Action(nameof(Put), new { id = m.id }),
                delete = Url.Action(nameof(Delete), new { id = m.id }),
                all = Url.Action(nameof(GetPaged), new { pageNumber = 1, pageSize = 10 })
            }
        });

        return Ok(new
        {
            result.TotalItems,
            result.PageNumber,
            result.PageSize,
            result.TotalPages,
            Items = motosComLinks
        });
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obtém uma moto por ID",
        Description = "Retorna uma moto com base no ID fornecido. Se não encontrar a moto, retorna 404 Not Found.",
        OperationId = "GetMotoById",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var moto = motoService.ObterPorId(id);
        if (moto == null)
            return NotFound();

        var response = new
        {
            moto.id,
            moto.tipoMoto,
            moto.placa,
            moto.numChassi,
            _links = new
            {
                self = Url.Action(nameof(Get), new { id = moto.id }),
                update = Url.Action(nameof(Put), new { id = moto.id }),
                delete = Url.Action(nameof(Delete), new { id = moto.id }),
                all = Url.Action(nameof(GetPaged), new { pageNumber = 1, pageSize = 10 })
            }
        };

        return Ok(response);
    }

   [HttpGet("tipo")]
[SwaggerOperation(
    Summary = "Obtém moto por tipo",
    Description = "Retorna uma moto filtrada por tipo. Se não encontrar, retorna 404 Not Found.",
    OperationId = "GetMotoByType",
    Tags = new[] { "Moto" }
)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public IActionResult GetPorTipo([FromQuery] string tipo)
{
    var moto = motoService.ObterPorTipo(tipo);
    if (moto == null)
        return NotFound();

    var motoComLinks = new
    {
        moto.id,
        moto.tipoMoto,
        moto.placa,
        moto.numChassi,
        _links = new
        {
            self = Url.Action(nameof(Get), new { id = moto.id }),
            update = Url.Action(nameof(Put), new { id = moto.id }),
            delete = Url.Action(nameof(Delete), new { id = moto.id })
        }
    };

    return Ok(motoComLinks);
}


  [HttpPost]
[SwaggerOperation(
    Summary = "Cadastra uma nova moto",
    Description = "Recebe um objeto de moto e cadastra uma nova moto. Retorna o objeto da moto criada com status 201 Created.",
    OperationId = "CreateMoto",
    Tags = new[] { "Moto" }
)]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public IActionResult Post([FromBody] Moto moto, [FromServices] MotoEventPublisher publisher)
{
    if (string.IsNullOrWhiteSpace(moto.tipoMoto) || string.IsNullOrWhiteSpace(moto.placa))
        return BadRequest("Tipo da moto e placa são obrigatórios.");

    var criada = motoService.CadastrarMoto(moto);

        // --- Mensageria ---
        publisher.PublishCreate(moto);
        // ------------------

        var response = new
    {
        criada.id,
        criada.tipoMoto,
        criada.placa,
        criada.numChassi,
        _links = new
        {
            self = Url.Action(nameof(Get), new { id = criada.id }),
            update = Url.Action(nameof(Put), new { id = criada.id }),
            delete = Url.Action(nameof(Delete), new { id = criada.id }),
            all = Url.Action(nameof(GetPaged), new { pageNumber = 1, pageSize = 10 })
        }
    };

    return CreatedAtAction(nameof(Get), new { id = criada.id }, response);
}

[HttpPut("{id}")]
[SwaggerOperation(
    Summary = "Atualiza uma moto existente",
    Description = "Atualiza os dados de uma moto existente. Retorna 204 No Content se a atualização for bem-sucedida. Caso contrário, retorna 404 Not Found.",
    OperationId = "UpdateMoto",
    Tags = new[] { "Moto" }
)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public IActionResult Put(int id, [FromBody] UpdateMoto dto, [FromServices] MotoEventPublisher publisher)
{
    if (dto == null)
        return BadRequest("Dados inválidos.");

    try
    {
        var atualizado = motoService.Atualizar(id, dto);

        if (!atualizado)
            return NotFound();

        // --- Mensageria ---
        var motoAtualizada = motoService.ObterPorId(id);
        publisher.PublishUpdate(motoAtualizada);
        // ------------------

        return NoContent();
    }
    catch (ValidationException ex)
    {
        return BadRequest(new { erros = ex.Errors });
    }
}
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove uma moto",
        Description = "Remove uma moto com base no ID fornecido. Retorna 204 No Content se a remoção for bem-sucedida. Caso contrário, retorna 404 Not Found.",
        OperationId = "DeleteMoto",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return motoService.Remover(id) ? NoContent() : NotFound();
    }
}
