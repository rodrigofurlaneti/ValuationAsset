using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ValuationAsset.Application.Queries;

namespace ValuationAsset.Api.Controllers
{
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
    }
}
