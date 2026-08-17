using MediatR;
using ValuationAsset.Application.Dtos;

namespace ValuationAsset.Application.Queries
{
    public record GetBazinValuationQuery : IRequest<List<BazinAnalysisDto>>;
}
