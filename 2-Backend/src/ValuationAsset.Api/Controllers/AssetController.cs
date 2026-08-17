using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ValuationAsset.Application.Queries;

namespace ValuationAsset.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna o snapshot fundamentalista mais recente de um ativo.
    /// Exemplo: GET /api/asset/ASAI3
    /// </summary>
    [HttpGet("{ticker}")]
    public async Task<IActionResult> GetValuation(string ticker)
    {
        var query = new GetAssetValuationQuery(ticker.ToUpperInvariant());
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Ativo '{ticker}' não encontrado no banco de dados." });

        return Ok(result);
    }

    /// <summary>
    /// Retorna a análise de Preço Justo de Graham e a Margem de Segurança para todos os ativos ativos.
    /// Exemplo: GET /api/asset/analysis/graham
    /// </summary>
    [HttpGet("analysis/graham")]
    public async Task<IActionResult> GetGrahamAnalysis()
    {
        var query = new GetGrahamValuationQuery();
        var result = await _mediator.Send(query);

        if (result == null || result.Count == 0)
            return Ok(new { message = "Nenhum ativo qualificado para a análise de Graham foi encontrado (lucros ou VPA negativos)." });

        return Ok(result);
    }
}