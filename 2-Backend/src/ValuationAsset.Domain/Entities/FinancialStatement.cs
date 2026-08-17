using System;

namespace ValuationAsset.Domain.Entities;

public class FinancialStatement
{
    public int StatementId { get; set; }
    public string StockTicker { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }

    // Balance Sheet (Patrimônio)
    public decimal TotalAssets { get; set; }
    public decimal LiquidAssets { get; set; }
    public decimal CurrentAssets { get; set; }
    public decimal GrossDebt { get; set; }
    public decimal NetDebt { get; set; }
    public decimal TotalEquity { get; set; }

    // Income Statement 12 Months (DRE 12m)
    public decimal YearlyRevenue { get; set; }
    public decimal YearlyEbit { get; set; }
    public decimal YearlyProfit { get; set; }

    // Income Statement 3 Months (DRE 3m)
    public decimal QuarterlyRevenue { get; set; }
    public decimal QuarterlyEbit { get; set; }
    public decimal QuarterlyProfit { get; set; }

    public long SharesCount { get; set; }

    // Propriedade de Navegação
    public virtual CompanyAsset? CompanyAsset { get; set; }
}