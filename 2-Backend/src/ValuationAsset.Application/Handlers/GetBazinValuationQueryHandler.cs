using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Dtos;
using ValuationAsset.Application.Queries;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Application.Handlers;

public class GetBazinValuationQueryHandler : IRequestHandler<GetBazinValuationQuery, List<BazinAnalysisDto>>
{
    private readonly ICompanyAssetRepository _repository;

    public GetBazinValuationQueryHandler(ICompanyAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<BazinAnalysisDto>> Handle(GetBazinValuationQuery request, CancellationToken cancellationToken)
    {
        var rawData = await _repository.GetRawDataForBazinAnalysisAsync(cancellationToken);
        var analysisResult = new List<BazinAnalysisDto>();

        foreach (var row in rawData)
        {
            decimal currentPrice = row.CurrentPrice;
            decimal dividendYield = row.DividendYield; // Ex: 0.08 (8%)

            // Estimativa do Dividendo por Ação (DPA) = Preço Atual * Dividend Yield
            decimal dpa = currentPrice * dividendYield;

            // Fórmula de Décio Bazin: Preço Teto com base em 6% ao ano (0.06)
            decimal fairPriceBazin = dpa > 0 ? Math.Round(dpa / 0.06m, 2) : 0;

            decimal safetyMargin = (currentPrice > 0 && fairPriceBazin > 0)
                ? Math.Round(((fairPriceBazin / currentPrice) - 1) * 100, 2)
                : 0;

            // Correção da sintaxe do switch expression
            string status = safetyMargin switch
            {
                >= 20 => "Excelente para Renda (Abaixo do Preço Teto)",
                > 0 and < 20 => "Preço Justo",
                _ => "Acima do Preço Teto (Menos atrativo para dividendos)"
            };

            analysisResult.Add(new BazinAnalysisDto(
                row.StockTicker,
                row.CompanyName,
                currentPrice,
                fairPriceBazin,
                safetyMargin,
                status
            ));
        }

        return analysisResult.OrderByDescending(a => a.SafetyMarginPercentage).ToList();
    }
}