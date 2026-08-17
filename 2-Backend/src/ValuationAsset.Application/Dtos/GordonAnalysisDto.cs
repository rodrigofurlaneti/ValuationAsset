namespace ValuationAsset.Application.Dtos
{
    public record GordonAnalysisDto(
        string StockTicker,
        string CompanyName,
        decimal CurrentPrice,
        decimal FairPriceGordon,
        decimal SafetyMarginPercentage,
        string ValuationStatus
    );
}
