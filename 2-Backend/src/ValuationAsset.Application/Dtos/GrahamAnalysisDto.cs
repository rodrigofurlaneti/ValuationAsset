using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValuationAsset.Application.Dtos
{
    public record GrahamAnalysisDto(
        string StockTicker,
        string CompanyName,
        decimal CurrentPrice,
        decimal FairPrice,
        decimal SafetyMarginPercentage,
        string ValuationStatus
    );
}
