using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ValuationAsset.Application.Interfaces;
using ValuationAsset.Domain.Repositories;
using ValuationAsset.Infrastructure.Data;
using ValuationAsset.Infrastructure.Repositories;
using ValuationAsset.Infrastructure.Services;

namespace ValuationAsset.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configura o DbContext com SQL Server
        services.AddDbContext<ValuationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ValuationDbContext).Assembly.FullName)));

        // 2. Registra os Repositórios
        services.AddScoped<ICompanyAssetRepository, CompanyAssetRepository>();
        services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();

        // 3. Registra o serviço de HTTP e o Scraper
        // O padrão AddHttpClient gerencia a vida útil das conexões web de forma otimizada
        services.AddHttpClient<IMarketScraperService, FundamentusScraperService>();

        return services;
    }
}