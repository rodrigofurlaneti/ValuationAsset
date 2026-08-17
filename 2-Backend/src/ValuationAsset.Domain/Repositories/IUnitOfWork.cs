using System.Threading;
using System.Threading.Tasks;

namespace ValuationAsset.Domain.Repositories
{
    public interface IUnitOfWork
    {
        Task<bool> CommitAsync(CancellationToken cancellationToken = default);
    }
}
