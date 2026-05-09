using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class RouteExecutionRepository : Repository<RouteExecution>, IRouteExecutionRepository
{
    public RouteExecutionRepository(ElisalDbContext context) : base(context)
    {
    }

    public async Task<RouteExecution?> GetActiveByDriverIdAsync(int driverId)
    {
        return await _dbSet
            .Include(e => e.Route)
            .Include(e => e.PointStatuses)
            .FirstOrDefaultAsync(e => e.DriverId == driverId && e.Status == RouteExecutionStatus.InProgress);
    }

    public async Task<IEnumerable<RouteExecution>> GetCompletedWithoutReceptionAsync(int? driverId = null)
    {
        // Fetch receptions to find which RouteExecutions are already processed
        var linkedRouteIds = await _context.WasteReceptions
            .Where(r => r.RouteExecutionId.HasValue)
            .Select(r => r.RouteExecutionId!.Value)
            .ToListAsync();

        var query = _dbSet
            .Include(e => e.Route)
            .Include(e => e.Driver)
            .Where(e => e.Status == RouteExecutionStatus.Completed && !linkedRouteIds.Contains(e.Id));

        if (driverId.HasValue)
        {
            query = query.Where(e => e.DriverId == driverId.Value);
        }

        return await query.OrderByDescending(e => e.EndTime).ToListAsync();
    }
}
