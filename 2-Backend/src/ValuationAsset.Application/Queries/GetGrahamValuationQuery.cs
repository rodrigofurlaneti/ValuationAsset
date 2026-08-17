using MediatR;
using System.Collections.Generic;

namespace ValuationAsset.Application.Queries
{
    public record GetGrahamValuationQuery() : IRequest<List<GrahamAnalysisDto>>;
}
