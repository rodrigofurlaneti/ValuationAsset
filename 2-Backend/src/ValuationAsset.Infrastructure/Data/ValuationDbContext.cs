using Microsoft.EntityFrameworkCore;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Infrastructure.Data
{
    public class ValuationDbContext : DbContext, IUnitOfWork
    {
        public ValuationDbContext(DbContextOptions<ValuationDbContext> options) : base(options) { }

        public DbSet<CompanyAsset> CompanyAssets => Set<CompanyAsset>();
        public DbSet<FinancialStatement> FinancialStatements => Set<FinancialStatement>();
        public DbSet<MarketQuote> MarketQuotes => Set<MarketQuote>();
        public DbSet<MarketIndicator> MarketIndicators => Set<MarketIndicator>();
        public DbSet<ExecutionLog> ExecutionLogs => Set<ExecutionLog>();

        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            // Salva as alterações no banco de dados
            return await base.SaveChangesAsync(cancellationToken) > 0;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Mapeamento CompanyAsset
            modelBuilder.Entity<CompanyAsset>(builder =>
            {
                builder.ToTable("CompanyAsset");
                builder.HasKey(c => c.StockTicker);
                builder.Property(c => c.StockTicker).HasMaxLength(10).IsRequired();
                builder.Property(c => c.CompanyName).HasMaxLength(100);
                builder.Property(c => c.AssetType).HasMaxLength(20);
                builder.Property(c => c.MarketSector).HasMaxLength(100);
                builder.Property(c => c.IndustryGroup).HasMaxLength(100);
            });

            // 2. Mapeamento FinancialStatement
            modelBuilder.Entity<FinancialStatement>(builder =>
            {
                builder.ToTable("FinancialStatement");
                builder.HasKey(f => f.StatementId);
                builder.Property(f => f.StockTicker).HasMaxLength(10).IsRequired();
                builder.Property(f => f.StatementDate).HasColumnType("date");

                // Configuração dos Decimais (18,2)
                var decimalProps = new[] {
                    nameof(FinancialStatement.TotalAssets), nameof(FinancialStatement.LiquidAssets),
                    nameof(FinancialStatement.CurrentAssets), nameof(FinancialStatement.GrossDebt),
                    nameof(FinancialStatement.NetDebt), nameof(FinancialStatement.TotalEquity),
                    nameof(FinancialStatement.YearlyRevenue), nameof(FinancialStatement.YearlyEbit),
                    nameof(FinancialStatement.YearlyProfit), nameof(FinancialStatement.QuarterlyRevenue),
                    nameof(FinancialStatement.QuarterlyEbit), nameof(FinancialStatement.QuarterlyProfit)
                };
                foreach (var prop in decimalProps)
                    builder.Property(prop).HasColumnType("decimal(18,2)");

                builder.HasIndex(f => new { f.StockTicker, f.StatementDate }).IsUnique();

                builder.HasOne(f => f.CompanyAsset)
                       .WithMany(c => c.FinancialStatements)
                       .HasForeignKey(f => f.StockTicker);
            });

            // 3. Mapeamento MarketQuote (Ampliado para decimal(18,2) para suportar valores de mercado bilionários)
            modelBuilder.Entity<MarketQuote>(builder =>
            {
                builder.ToTable("MarketQuote");
                builder.HasKey(m => m.QuoteId);
                builder.Property(m => m.StockTicker).HasMaxLength(10).IsRequired();
                builder.Property(m => m.ReferenceDate).HasColumnType("date");
                builder.Property(m => m.ClosingPrice).HasColumnType("decimal(18,2)");
                builder.Property(m => m.AverageVolume).HasColumnType("decimal(18,2)");
                builder.Property(m => m.MarketValue).HasColumnType("decimal(18,2)");
                builder.Property(m => m.FirmValue).HasColumnType("decimal(18,2)");

                builder.HasIndex(m => new { m.StockTicker, m.ReferenceDate }).IsUnique();

                builder.HasOne(m => m.CompanyAsset)
                       .WithMany(c => c.MarketQuotes)
                       .HasForeignKey(m => m.StockTicker);
            });

            // 4. Mapeamento MarketIndicator (Ampliado para decimal(18,4) para evitar overflow em múltiplos e percentuais)
            modelBuilder.Entity<MarketIndicator>(builder =>
            {
                builder.ToTable("MarketIndicator");
                builder.HasKey(m => m.IndicatorId);
                builder.Property(m => m.StockTicker).HasMaxLength(10).IsRequired();
                builder.Property(m => m.ReferenceDate).HasColumnType("date");

                builder.Property(m => m.PriceEarnings).HasColumnType("decimal(18,4)");
                builder.Property(m => m.PriceBook).HasColumnType("decimal(18,4)");
                builder.Property(m => m.EnterpriseEbitda).HasColumnType("decimal(18,4)");
                builder.Property(m => m.EarningsShare).HasColumnType("decimal(18,4)");
                builder.Property(m => m.BookShare).HasColumnType("decimal(18,4)");

                builder.Property(m => m.DividendYield).HasColumnType("decimal(18,4)");
                builder.Property(m => m.CapitalReturn).HasColumnType("decimal(18,4)");
                builder.Property(m => m.EquityReturn).HasColumnType("decimal(18,4)");
                builder.Property(m => m.NetMargin).HasColumnType("decimal(18,4)");

                builder.HasIndex(m => new { m.StockTicker, m.ReferenceDate }).IsUnique();

                builder.HasOne(m => m.CompanyAsset)
                       .WithMany(c => c.MarketIndicators)
                       .HasForeignKey(m => m.StockTicker);
            });

            // 5. Mapeamento ExecutionLog
            modelBuilder.Entity<ExecutionLog>(builder =>
            {
                builder.ToTable("ExecutionLog");
                builder.HasKey(e => e.LogId);
                builder.Property(e => e.ExecutionTime).HasColumnType("datetime2").IsRequired();
                builder.Property(e => e.ProcessStatus).HasMaxLength(50).IsRequired();
                builder.Property(e => e.LogMessage).HasColumnType("varchar(max)");
                builder.Property(e => e.RecordsAffected).HasDefaultValue(0);
            });
        }
    }
}