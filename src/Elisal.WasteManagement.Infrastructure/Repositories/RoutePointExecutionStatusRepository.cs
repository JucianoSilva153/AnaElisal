using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class RoutePointExecutionStatusRepository : Repository<RoutePointExecutionStatus>, IRoutePointExecutionStatusRepository
{
    public RoutePointExecutionStatusRepository(ElisalDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RoutePointExecutionStatus>> GetPendingByExecutionIdAsync(int executionId)
    {
        return await _context.RoutePointExecutionStatuses
            .Include(s => s.CollectionPoint)
            .Where(s => s.RouteExecutionId == executionId
                     && !s.IsCompleted
                     && !s.IsSkipped
                     && s.CollectionPoint != null)
            .ToListAsync();
    }

    public async Task<bool> HasAnyPendingAtPointAsync(int executionId, int collectionPointId)
    {
        return await _context.RoutePointExecutionStatuses
            .AnyAsync(s => s.RouteExecutionId == executionId
                       && s.CollectionPointId == collectionPointId
                       && !s.IsCompleted
                       && !s.IsSkipped);
    }
}
