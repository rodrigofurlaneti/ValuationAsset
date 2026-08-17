using Microsoft.EntityFrameworkCore;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;
using ValuationAsset.Infrastructure.Data;

namespace ValuationAsset.Infrastructure.Repositories
{
    public class CompanyAssetRepository : ICompanyAssetRepository
    {
        private readonly ValuationDbContext _dbContext;

        public CompanyAssetRepository(ValuationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IUnitOfWork UnitOfWork => _dbContext;

        public async Task<CompanyAsset?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
        {
            // Inclui os dados mais recentes de balanço e cotação para validação do Delta (Has New Data)
            return await _dbContext.CompanyAssets
                .Include(c => c.FinancialStatements.OrderByDescending(f => f.StatementDate).Take(1))
                .Include(c => c.MarketQuotes.OrderByDescending(m => m.ReferenceDate).Take(1))
                .FirstOrDefaultAsync(c => c.StockTicker == ticker, cancellationToken);
        }

        public async Task AddAsync(CompanyAsset asset, CancellationToken cancellationToken = default)
        {
            await _dbContext.CompanyAssets.AddAsync(asset, cancellationToken);
        }

        public void Update(CompanyAsset asset)
        {
            _dbContext.CompanyAssets.Update(asset);
        }
    }
2.
}
