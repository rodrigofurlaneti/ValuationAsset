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

        public async Task<IEnumerable<(string StockTicker, string CompanyName, decimal CurrentPrice, decimal DividendYield)>> GetRawDataForBazinAnalysisAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
        WITH UltimaCotacao AS (
            SELECT StockTicker, ClosingPrice, ReferenceDate,
                   ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY ReferenceDate DESC) as rn
            FROM MarketQuote
            WHERE YEAR(ReferenceDate) = 2026
        ),
        UltimoIndicador AS (
            SELECT StockTicker, DividendYield, ReferenceDate,
                   ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY ReferenceDate DESC) as rn
            FROM MarketIndicator
        )
        SELECT 
            c.StockTicker,
            c.CompanyName,
            q.ClosingPrice AS CurrentPrice,
            i.DividendYield AS DividendYield
        FROM CompanyAsset c
        JOIN UltimaCotacao q ON c.StockTicker = q.StockTicker AND q.rn = 1
        JOIN UltimoIndicador i ON c.StockTicker = i.StockTicker AND i.rn = 1
        WHERE i.DividendYield > 0 AND q.ClosingPrice > 0;
    ";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<(string, string, decimal, decimal)>(sql);
        }

        public async Task<IEnumerable<(string StockTicker, string CompanyName, decimal CurrentPrice, decimal DPA, decimal DividendGrowthRate)>> GetRawDataForGordonAnalysisAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
        WITH UltimaCotacao AS (
            SELECT StockTicker, ClosingPrice,
                   ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY (SELECT NULL)) as rn
            FROM MarketQuote
        ),
        UltimoIndicador AS (
            SELECT StockTicker, DividendYield,
                   ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY (SELECT NULL)) as rn
            FROM MarketIndicator
        )
        SELECT 
            c.StockTicker,
            c.CompanyName,
            ISNULL(q.ClosingPrice, 0) AS CurrentPrice,
            (ISNULL(q.ClosingPrice, 0) * ISNULL(i.DividendYield, 0)) AS DPA,
            0.05 AS DividendGrowthRate -- Valor fixo de 5% para evitar erro de coluna
        FROM CompanyAsset c
        JOIN UltimaCotacao q ON c.StockTicker = q.StockTicker AND q.rn = 1
        JOIN UltimoIndicador i ON c.StockTicker = i.StockTicker AND i.rn = 1
        WHERE q.ClosingPrice > 0 AND i.DividendYield > 0;";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<(string, string, decimal, decimal, decimal)>(sql);
        }

        public async Task<IEnumerable<(string StockTicker, string CompanyName, decimal EarningsYield, decimal ROIC, int Ranking)>> GetMagicFormulaRankingAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
        WITH UltimaCotacao AS (
            SELECT StockTicker, MarketValue,
                   ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY (SELECT NULL)) as rn
            FROM MarketQuote
        ),
        UltimoFinanceiro AS (
            SELECT StockTicker, YearlyEbit, NetDebt, TotalEquity,
                   ROW_NUMBER() OVER(PARTITION BY StockTicker ORDER BY (SELECT NULL)) as rn
            FROM FinancialStatement
        ),
        DadosBasicos AS (
            SELECT 
                c.StockTicker, c.CompanyName,
                CAST(f.YearlyEbit AS DECIMAL(18,4)) / NULLIF(CAST((q.MarketValue + ISNULL(f.NetDebt, 0)) AS DECIMAL(18,4)), 0) AS EarningsYield,
                CAST(f.YearlyEbit AS DECIMAL(18,4)) / NULLIF(CAST((f.TotalEquity + ISNULL(f.NetDebt, 0)) AS DECIMAL(18,4)), 0) AS ROIC
            FROM CompanyAsset c
            JOIN UltimoFinanceiro f ON c.StockTicker = f.StockTicker AND f.rn = 1
            JOIN UltimaCotacao q ON c.StockTicker = q.StockTicker AND q.rn = 1
            WHERE f.YearlyEbit > 0 
              AND q.MarketValue > 0
              AND f.TotalEquity > 0
        ),
        Ranking AS (
            SELECT *,
                   RANK() OVER (ORDER BY EarningsYield DESC) as RankEY,
                   RANK() OVER (ORDER BY ROIC DESC) as RankROIC
            FROM DadosBasicos
            WHERE EarningsYield IS NOT NULL AND ROIC IS NOT NULL
        )
        SELECT StockTicker, CompanyName, EarningsYield, ROIC, (RankEY + RankROIC) as Ranking
        FROM Ranking
        ORDER BY Ranking ASC;";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<(string, string, decimal, decimal, int)>(sql);
        }

        public void Update(CompanyAsset asset)
        {
            _dbContext.CompanyAssets.Update(asset);
        }
    }
}