using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Commands;
using ValuationAsset.Application.Interfaces;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Application.Handlers
{
    public class SyncMarketDataCommandHandler : IRequestHandler<SyncMarketDataCommand, bool>
    {
        private readonly IMarketScraperService _scraperService;
        private readonly ICompanyAssetRepository _assetRepository;
        private readonly IExecutionLogRepository _logRepository;

        public SyncMarketDataCommandHandler(
            IMarketScraperService scraperService,
            ICompanyAssetRepository assetRepository,
            IExecutionLogRepository logRepository)
        {
            _scraperService = scraperService;
            _assetRepository = assetRepository;
            _logRepository = logRepository;
        }

        public async Task<bool> Handle(SyncMarketDataCommand request, CancellationToken cancellationToken)
        {
            // 1. Coleta de Dados (Web Scraping)
            var scrapedData = await _scraperService.ScrapeAssetDataAsync(request.StockTicker, cancellationToken);
            if (scrapedData == null)
                return await LogAndReturn(false, "ERROR", $"Falha ao extrair dados para {request.StockTicker}", 0, cancellationToken);

            // 2. Busca o registro atual no banco de dados
            var existingAsset = await _assetRepository.GetByTickerAsync(request.StockTicker, cancellationToken);

            // 3. Validação de Delta (Has New Data?)
            if (existingAsset != null)
            {
                var latestQuote = existingAsset.MarketQuotes.FirstOrDefault();

                // Verifica se a cotação de hoje já está no banco e se o preço não mudou
                bool isSameDate = latestQuote?.ReferenceDate == scrapedData.Quote.ReferenceDate;
                bool isSamePrice = latestQuote?.ClosingPrice == scrapedData.Quote.ClosingPrice;

                if (isSameDate && isSamePrice)
                {
                    // Caminho A: Sem novidades. Registra no log e aborta.
                    return await LogAndReturn(true, "SKIPPED", $"Sem dados novos para {request.StockTicker}", 0, cancellationToken);
                }
            }

            // Caminho B: Dados novos encontrados. Preparando para salvar.
            int recordsAffected = 0;

            if (existingAsset == null)
            {
                // É uma empresa nova no banco
                var newAsset = scrapedData.Company;
                newAsset.FinancialStatements.Add(scrapedData.Statement);
                newAsset.MarketQuotes.Add(scrapedData.Quote);
                newAsset.MarketIndicators.Add(scrapedData.Indicator);

                await _assetRepository.AddAsync(newAsset, cancellationToken);
                recordsAffected = 4;
            }
            else
            {
                // Empresa já existe, apenas adiciona o snapshot diário/trimestral
                existingAsset.MarketQuotes.Add(scrapedData.Quote);
                existingAsset.MarketIndicators.Add(scrapedData.Indicator);
                recordsAffected = 2;

                // Só adiciona um novo balanço se a data for diferente do último salvo
                if (!existingAsset.FinancialStatements.Any(f => f.StatementDate == scrapedData.Statement.StatementDate))
                {
                    existingAsset.FinancialStatements.Add(scrapedData.Statement);
                    recordsAffected++;
                }

                _assetRepository.Update(existingAsset);
            }

            // 4. Atualização Transacional (Commit)
            await _assetRepository.UnitOfWork.CommitAsync(cancellationToken);

            // 5. Registro de Sucesso
            return await LogAndReturn(true, "SUCCESS", $"Dados sincronizados com sucesso para {request.StockTicker}", recordsAffected, cancellationToken);
        }

        private async Task<bool> LogAndReturn(bool result, string status, string message, int records, CancellationToken cancellationToken)
        {
            var log = new ExecutionLog
            {
                ExecutionTime = DateTime.UtcNow, // Ou DateTime.Now dependendo do fuso do servidor
                ProcessStatus = status,
                LogMessage = message,
                RecordsAffected = records
            };

            await _logRepository.AddAsync(log, cancellationToken);
            await _logRepository.UnitOfWork.CommitAsync(cancellationToken);

            return result;
        }
    }
}
