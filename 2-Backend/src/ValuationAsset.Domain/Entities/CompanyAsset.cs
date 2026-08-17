using System.Collections.Generic;

namespace ValuationAsset.Domain.Entities;

public class CompanyAsset
{
    public string StockTicker { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string MarketSector { get; set; } = string.Empty;
    public string IndustryGroup { get; set; } = string.Empty;

    // Propriedades de Navegação (Relacionamentos)
    public virtual ICollection<FinancialStatement> FinancialStatements { get; set; } = new List<FinancialStatement>();
    public virtual ICollection<MarketQuote> MarketQuotes { get; set; } = new List<MarketQuote>();
    public virtual ICollection<MarketIndicator> MarketIndicators { get; set; } = new List<MarketIndicator>();
}