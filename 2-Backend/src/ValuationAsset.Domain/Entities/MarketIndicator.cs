using System;

namespace ValuationAsset.Domain.Entities;

public class MarketIndicator
{
    public int IndicatorId { get; set; }
    public string StockTicker { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }

    // Valuation Indicators
    public decimal PriceEarnings { get; set; }
    public decimal PriceBook { get; set; }
    public decimal EnterpriseEbitda { get; set; }
    public decimal DividendYield { get; set; }

    // Per Share Indicators
    public decimal EarningsShare { get; set; }
    public decimal BookShare { get; set; }

    // Profitability Indicators
    public decimal CapitalReturn { get; set; }
    public decimal EquityReturn { get; set; }
    public decimal NetMargin { get; set; }

    // Propriedade de Navegação
    public virtual CompanyAsset? CompanyAsset { get; set; }
}