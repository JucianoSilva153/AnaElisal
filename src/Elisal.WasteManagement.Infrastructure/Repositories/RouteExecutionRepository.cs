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
}
