using MediatR;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Application.Queries
{
    public record GetAssetValuationQuery(string StockTicker) : IRequest<CompanyAsset?>;
}
