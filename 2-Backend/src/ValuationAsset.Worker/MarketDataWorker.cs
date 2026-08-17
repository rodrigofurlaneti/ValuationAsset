using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Commands;

namespace ValuationAsset.Worker
{
    public class MarketDataWorker : BackgroundService
    {
        private readonly ILogger<MarketDataWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        // O MediatR precisa de um escopo para resolver o DbContext corretamente,
        // por isso injetamos o IServiceProvider em vez do IMediator direto.
        public MarketDataWorker(ILogger<MarketDataWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ValuationAsset Worker iniciado às: {time}", DateTimeOffset.Now);

            // O loop infinito do Background Service
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Iniciando ciclo de sincronização...");

                    // Cria um escopo de injeção de dependência para esta execução
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        // Dispara o Command para o ativo ASAI3 (pode ser uma lista no futuro)
                        var command = new SyncMarketDataCommand("ASAI3");
                        var success = await mediator.Send(command, stoppingToken);

                        if (success)
                            _logger.LogInformation("Ciclo finalizado com sucesso.");
                        else
                            _logger.LogWarning("Ciclo finalizado, mas houve algum pulo (skipped) ou erro. Cheque o banco de dados.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro fatal durante a execução do ciclo.");
                }

                // Aguarda exatamente 1 minuto antes de rodar o próximo ciclo
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
