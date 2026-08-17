using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;
using ValuationAsset.Infrastructure.Data;

namespace ValuationAsset.Infrastructure.Repositories
{
    public class CompanyAssetRepository : ICompanyAssetRepository
    {
        private readonly ValuationDbContext _dbContext;
        private readonly string _connectionString;

        public CompanyAssetRepository(ValuationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new System.ArgumentNullException("Connection string not found.");
        }

        public IUnitOfWork UnitOfWork => _dbContext;

        public async Task<List<string>> GetAllTickersAsync(CancellationToken cancellationToken = default)
        {
            // Retorna apenas os nomes dos papéis (Tickers) para economizar memória
            return await _dbContext.CompanyAssets
                .Select(c => c.StockTicker)
                .ToListAsync(cancellationToken);
        }

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

        public async Task<IEnumerable<(string StockTicker, string CompanyName, decimal CurrentPrice, decimal LPA, decimal VPA)>> GetRawDataForGrahamAnalysisAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
            WITH UltimaCotacao AS (
                SELECT StockTicker, ClosingPrice, ReferenceDate,
                       ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY ReferenceDate DESC) as rn
                FROM MarketQuote
                WHERE YEAR(ReferenceDate) = 2026 -- FILTRO: Apenas cotações deste ano
            ),
            UltimoIndicador AS (
                SELECT StockTicker, EarningsShare, BookShare, ReferenceDate,
                       ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY ReferenceDate DESC) as rn
                FROM MarketIndicator
            ),
            CalculoGraham AS (
                SELECT 
                    c.StockTicker,
                    c.CompanyName,
                    q.ClosingPrice AS CurrentPrice,
                    i.EarningsShare AS LPA,
                    i.BookShare AS VPA,
                    -- Preço Justo de Graham: Raiz quadrada de (22.5 * LPA * VPA)
                    SQRT(22.5 * i.EarningsShare * i.BookShare) AS PrecoJusto
                FROM CompanyAsset c
                JOIN UltimaCotacao q ON c.StockTicker = q.StockTicker AND q.rn = 1
                JOIN UltimoIndicador i ON c.StockTicker = i.StockTicker AND i.rn = 1
                WHERE i.EarningsShare > 0 AND i.BookShare > 0 AND q.ClosingPrice > 0
            )
            SELECT 
                StockTicker,
                CompanyName,
                CurrentPrice,
                LPA,
                VPA
            FROM CalculoGraham
            ORDER BY (PrecoJusto / CurrentPrice) DESC;
        ";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<(string, string, decimal, decimal, decimal)>(sql);
        }

        public void Update(CompanyAsset asset)
        {
            _dbContext.CompanyAssets.Update(asset);
        }
    }
}