using System;
using System.Collections.Generic;
using System.Text;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Application.Interfaces
{
    public record ScrapedMarketData(
        CompanyAsset Company,
        FinancialStatement Statement,
        MarketQuote Quote,
        MarketIndicator Indicator
    );
}
