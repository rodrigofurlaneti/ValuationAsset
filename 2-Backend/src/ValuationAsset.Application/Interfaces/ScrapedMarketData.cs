using System;
using System.Collections.Generic;
using System.Text;

namespace ValuationAsset.Application.Interfaces
{
    public record ScrapedMarketData(
        CompanyAsset Company,
        FinancialStatement Statement,
        MarketQuote Quote,
        MarketIndicator Indicator
    );
}
