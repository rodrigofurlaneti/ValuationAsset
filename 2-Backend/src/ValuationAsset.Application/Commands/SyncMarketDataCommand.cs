using MediatR;

namespace ValuationAsset.Application.Commands
{
    public record SyncMarketDataCommand(string StockTicker) : IRequest<bool>;
}
