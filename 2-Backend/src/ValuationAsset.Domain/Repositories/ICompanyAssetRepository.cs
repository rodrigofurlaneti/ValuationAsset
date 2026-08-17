using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Domain.Repositories
{
    public interface ICompanyAssetRepository
    {
        IUnitOfWork UnitOfWork { get; }

        Task<CompanyAsset?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);
        Task AddAsync(CompanyAsset asset, CancellationToken cancellationToken = default);
        void Update(CompanyAsset asset);
    }
}
