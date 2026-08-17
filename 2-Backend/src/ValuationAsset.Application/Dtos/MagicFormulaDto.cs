namespace ValuationAsset.Application.Dtos
{
    public record MagicFormulaDto(
        string StockTicker,
        string CompanyName,
        decimal EarningsYield,
        decimal ROIC,
        int Ranking
    );
}
