using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValuationAsset.Application.Dtos;

namespace ValuationAsset.Application.Queries
{
    public record GetGordonValuationQuery : IRequest<List<GordonAnalysisDto>>;
}
