using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Queries;
using ValuationAsset.Application.Dtos;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Application.Handlers
{
    public class GetMagicFormulaQueryHandler : IRequestHandler<GetMagicFormulaQuery, List<MagicFormulaDto>>
    {
        private readonly ICompanyAssetRepository _repository;

        public GetMagicFormulaQueryHandler(ICompanyAssetRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MagicFormulaDto>> Handle(GetMagicFormulaQuery request, CancellationToken cancellationToken)
        {
            var rawData = await _repository.GetMagicFormulaRankingAsync(cancellationToken);

            return rawData.Select(r => new MagicFormulaDto(
                r.StockTicker,
                r.CompanyName,
                Math.Round(r.EarningsYield * 100, 2),
                Math.Round(r.ROIC * 100, 2),
                r.Ranking
            )).ToList();
        }
    }
}