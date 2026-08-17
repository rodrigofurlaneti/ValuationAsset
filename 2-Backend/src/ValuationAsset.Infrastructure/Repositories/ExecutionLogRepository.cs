using Microsoft.EntityFrameworkCore;
using ValuationAsset.Domain.Entities;
using ValuationAsset.Domain.Repositories;
using ValuationAsset.Infrastructure.Data;

namespace ValuationAsset.Infrastructure.Repositories
{
    public class ExecutionLogRepository : IExecutionLogRepository
    {
        private readonly ValuationDbContext _dbContext;

        public ExecutionLogRepository(ValuationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IUnitOfWork UnitOfWork => _dbContext;

        public async Task<ExecutionLog?> GetLastSuccessfulExecutionAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ExecutionLogs
                .Where(e => e.ProcessStatus == "SUCCESS")
                .OrderByDescending(e => e.ExecutionTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(ExecutionLog log, CancellationToken cancellationToken = default)
        {
            await _dbContext.ExecutionLogs.AddAsync(log, cancellationToken);
        }
    }
}
