using MediatR;
using System.Collections.Generic;
using ValuationAsset.Application.Dtos;

namespace ValuationAsset.Application.Queries
{
    public record GetGrahamValuationQuery() : IRequest<List<GrahamAnalysisDto>>;
}
