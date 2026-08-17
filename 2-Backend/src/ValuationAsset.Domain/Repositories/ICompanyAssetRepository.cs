using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Domain.Repositories
{
    public interface ICompanyAssetRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Task<CompanyAsset?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllTickersAsync(CancellationToken cancellationToken = default); // NOVO MÉTODO
        Task AddAsync(CompanyAsset asset, CancellationToken cancellationToken = default);
        void Update(CompanyAsset asset);
        Task<IEnumerable<(string StockTicker, string CompanyName, decimal CurrentPrice, decimal LPA, decimal VPA)>> GetRawDataForGrahamAnalysisAsync(CancellationToken cancellationToken = default);
    }
}
