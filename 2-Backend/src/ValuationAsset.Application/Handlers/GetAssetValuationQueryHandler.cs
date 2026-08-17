using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Queries;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;

namespace ValuationAsset.Application.Handlers
{
    public class GetAssetValuationQueryHandler : IRequestHandler<GetAssetValuationQuery, CompanyAsset?>
    {
        private readonly ICompanyAssetRepository _repository;

        public GetAssetValuationQueryHandler(ICompanyAssetRepository repository)
        {
            _repository = repository;
        }

        public async Task<CompanyAsset?> Handle(GetAssetValuationQuery request, CancellationToken cancellationToken)
        {
            // Usa o repositório existente para buscar os dados.
            // Como o repositório já inclui um "Take(1)" na cotação, 
            // ele vai retornar sempre a foto mais recente.
            return await _repository.GetByTickerAsync(request.StockTicker, cancellationToken);
        }
    }
}
