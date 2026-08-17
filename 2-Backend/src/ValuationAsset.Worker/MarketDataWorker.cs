using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Commands;
using ValuationAsset.Application.Interfaces;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Worker
{
    public class MarketDataWorker : BackgroundService
    {
        private readonly ILogger<MarketDataWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MarketDataWorker(ILogger<MarketDataWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ValuationAsset Worker iniciado às: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("==============================================");
                    _logger.LogInformation("Iniciando varredura geral de ativos da B3...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var scraper = scope.ServiceProvider.GetRequiredService<IMarketScraperService>();
                        var repository = scope.ServiceProvider.GetRequiredService<ICompanyAssetRepository>();

                        // 1. Busca dinamicamente todos os tickers ativos direto da B3 via Fundamentus
                        var allTickers = await scraper.GetAllActiveTickersAsync(stoppingToken);

                        if (!allTickers.Any())
                        {
                            _logger.LogWarning("Não foi possível recuperar a lista de ativos da B3. Tentarei novamente no próximo ciclo.");
                        }
                        else
                        {
                            _logger.LogInformation("Encontrados {count} ativos listados na B3.", allTickers.Count);

                            // 2. Garante que todos os ativos descobertos estejam cadastrados na tabela CompanyAsset do banco
                            var existingTickers = await repository.GetAllTickersAsync(stoppingToken);
                            var newTickers = allTickers.Except(existingTickers).ToList();

                            foreach (var newTicker in newTickers)
                            {
                                // Cadastra preventivamente a empresa base na tabela pai para evitar erro de Foreign Key
                                await repository.AddAsync(new CompanyAsset { StockTicker = newTicker, CompanyName = newTicker }, stoppingToken);
                            }

                            if (newTickers.Count > 0)
                            {
                                await repository.UnitOfWork.CommitAsync(stoppingToken);
                                _logger.LogInformation("{count} novos ativos adicionados à base de dados.", newTickers.Count);
                            }

                            // 3. Processa a sincronização de dados (Cotação, Balanços, Indicadores) de todos os ativos
                            // Dica: Para o primeiro teste completo, você pode limitar com .Take(20) para não demorar muito
                            foreach (var ticker in allTickers)
                            {
                                _logger.LogInformation("Sincronizando dados de: {ticker}", ticker);

                                var command = new SyncMarketDataCommand(ticker);
                                await mediator.Send(command, stoppingToken);

                                // Pausa de 1.5 segundo entre as requisições para respeitar o servidor de origem
                                await Task.Delay(1500, stoppingToken);
                            }
                        }

                        _logger.LogInformation("Varredura completa de todos os ativos finalizada.");
                        _logger.LogInformation("==============================================");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro fatal durante a varredura global.");
                }

                // Aguarda 1 hora antes de rodar a varredura completa novamente (ou o tempo que preferir)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}