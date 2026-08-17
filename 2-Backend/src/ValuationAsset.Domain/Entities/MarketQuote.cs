using System;

namespace ValuationAsset.Domain.Entities;

public class MarketQuote
{
    public int QuoteId { get; set; }
    public string StockTicker { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }

    public decimal ClosingPrice { get; set; }
    public decimal AverageVolume { get; set; }
    public decimal MarketValue { get; set; }
    public decimal FirmValue { get; set; }

    // Propriedade de Navegação
    public virtual CompanyAsset? CompanyAsset { get; set; }
}