using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Queries;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Application.Handlers
{
    public class GetGrahamValuationQueryHandler : IRequestHandler<GetGrahamValuationQuery, List<GrahamAnalysisDto>>
    {
        private readonly ICompanyAssetRepository _repository;

        // Injetamos o repositório em vez de usar conexão SQL direta
        public GetGrahamValuationQueryHandler(ICompanyAssetRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<GrahamAnalysisDto>> Handle(GetGrahamValuationQuery request, CancellationToken cancellationToken)
        {
            // Pede os dados brutos já tratados para a Infraestrutura
            var rawData = await _repository.GetRawDataForGrahamAnalysisAsync(cancellationToken);

            var analysisResult = new List<GrahamAnalysisDto>();

            foreach (var row in rawData)
            {
                decimal lpa = row.LPA;
                decimal vpa = row.VPA;
                decimal currentPrice = row.CurrentPrice;

                // Aplica a fórmula de Benjamin Graham (Regra de Negócio)
                decimal fairPrice = (decimal)Math.Round(Math.Sqrt(22.5 * (double)lpa * (double)vpa), 2);
                decimal safetyMargin = (currentPrice > 0)
                    ? Math.Round(((fairPrice / currentPrice) - 1) * 100, 2)
                    : 0;

                // Classifica o status da ação
                string status = safetyMargin switch
                {
                    >= 20 => "Descontada (Barata)",
                    > 0 and < 20 => "Preço Justo (Equilibrada)",
                    _ => "Cara (Prêmio embasado em crescimento)"
                };

                analysisResult.Add(new GrahamAnalysisDto(
                    row.StockTicker,
                    row.CompanyName,
                    currentPrice,
                    fairPrice,
                    safetyMargin,
                    status
                ));
            }

            // Retorna ordenado pela maior margem de segurança
            return analysisResult.OrderByDescending(a => a.SafetyMarginPercentage).ToList();
        }
    }
}