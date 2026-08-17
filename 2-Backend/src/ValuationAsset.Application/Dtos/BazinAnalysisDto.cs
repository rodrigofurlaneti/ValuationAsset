using MediatR;

namespace ValuationAsset.Application.Dtos
{
    public record BazinAnalysisDto(
        string StockTicker,
        string CompanyName,
        decimal CurrentPrice,
        decimal FairPriceBazin,
        decimal SafetyMarginPercentage,
        string ValuationStatus
    ) : IRequest; // ou retorna List<BazinAnalysisDto>
}
