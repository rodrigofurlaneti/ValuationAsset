using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ValuationAsset.Application;
using ValuationAsset.Infrastructure;
using ValuationAsset.Worker;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    // 1. Registra as dependências da Aplicação (MediatR)
    services.AddApplication();

    // 2. Registra as dependências da Infraestrutura (EF Core, Repositórios, Scraper)
    services.AddInfrastructure(hostContext.Configuration);

    // 3. Registra o serviço que vai rodar em background
    services.AddHostedService<MarketDataWorker>();
});

var host = builder.Build();
host.Run();