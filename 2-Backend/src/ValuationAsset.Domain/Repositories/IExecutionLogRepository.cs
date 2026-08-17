using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Domain.Repositories
{
    public interface IExecutionLogRepository
    {
        IUnitOfWork UnitOfWork { get; }

        Task<ExecutionLog?> GetLastSuccessfulExecutionAsync(CancellationToken cancellationToken = default);
        Task AddAsync(ExecutionLog log, CancellationToken cancellationToken = default);
    }
}
