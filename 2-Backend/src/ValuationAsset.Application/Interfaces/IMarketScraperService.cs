using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Application.Interfaces
{
    public interface IMarketScraperService
    {
        Task<ScrapedMarketData?> ScrapeAssetDataAsync(string ticker, CancellationToken cancellationToken);
        Task<List<string>> GetAllActiveTickersAsync(CancellationToken cancellationToken);
    }
}
