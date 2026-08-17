using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValuationAsset.Application.Dtos;
using ValuationAsset.Application.Queries;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Application.Handlers
{
    public class GetGordonValuationQueryHandler : IRequestHandler<GetGordonValuationQuery, List<GordonAnalysisDto>>
    {
        private readonly ICompanyAssetRepository _repository;

        public GetGordonValuationQueryHandler(ICompanyAssetRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<GordonAnalysisDto>> Handle(GetGordonValuationQuery request, CancellationToken cancellationToken)
        {
            var rawData = await _repository.GetRawDataForGordonAnalysisAsync(cancellationToken);
            var result = new List<GordonAnalysisDto>();

            // Taxa de retorno exigida pelo investidor (ex: 12% ao ano)
            decimal requiredReturn = 0.12m;

            foreach (var row in rawData)
            {
                decimal currentPrice = row.CurrentPrice;
                decimal dpa = row.DPA;
                decimal growthRate = row.DividendGrowthRate; // Ex: 0.05 (5%)

                // Proteção para evitar divisão por zero ou taxa de crescimento maior que a exigida
                if (growthRate >= requiredReturn || (requiredReturn - growthRate) <= 0)
                    continue;

                // Fórmula de Gordon: Preço Justo = (DPA * (1 + g)) / (r - g)
                decimal nextYearDpa = dpa * (1 + growthRate);
                decimal fairPriceGordon = Math.Round(nextYearDpa / (requiredReturn - growthRate), 2);

                decimal safetyMargin = currentPrice > 0
                    ? Math.Round(((fairPriceGordon / currentPrice) - 1) * 100, 2)
                    : 0;

                string status = safetyMargin switch
                {
                    >= 20 => "Descontada (Excelente Valor)",
                    > 0 and < 20 => "Preço Justo",
                    _ => "Acima do Preço Justo"
                };

                result.Add(new GordonAnalysisDto(
                    row.StockTicker,
                    row.CompanyName,
                    currentPrice,
                    fairPriceGordon,
                    safetyMargin,
                    status
                ));
            }

            return result.OrderByDescending(a => a.SafetyMarginPercentage).ToList();
        }
    }
}
