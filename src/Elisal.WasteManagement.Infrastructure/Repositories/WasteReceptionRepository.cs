using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class WasteReceptionRepository : Repository<WasteReception>, IWasteReceptionRepository
{
    public WasteReceptionRepository(ElisalDbContext context) : base(context) { }

    public async Task<IEnumerable<WasteReception>> GetByPeriodAsync(DateTime start, DateTime end)
    {
        return await _context.WasteReceptions
            .Include(r => r.SortingCenter)
            .Include(r => r.ReceivedByUser)
            .Include(r => r.RouteExecution)
                .ThenInclude(re => re!.Route)
            .Where(r => r.DateTime >= start && r.DateTime <= end)
            .OrderByDescending(r => r.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WasteReception>> GetBySortingCenterAsync(int sortingCenterId)
    {
        return await _context.WasteReceptions
            .Include(r => r.SortingCenter)
            .Include(r => r.ReceivedByUser)
            .Where(r => r.SortingCenterId == sortingCenterId)
            .OrderByDescending(r => r.DateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WasteReception>> GetPendingSortingAsync()
    {
        return await _context.WasteReceptions
            .Include(r => r.SortingCenter)
            .Include(r => r.ReceivedByUser)
            .Where(r => !r.IsSorted)
            .OrderByDescending(r => r.DateTime)
            .ToListAsync();
    }

    public async Task<WasteReception?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.WasteReceptions
            .Include(r => r.SortingCenter)
            .Include(r => r.ReceivedByUser)
            .Include(r => r.RouteExecution)
                .ThenInclude(re => re!.Route)
            .Include(r => r.SortingBatches)
                .ThenInclude(b => b.Outputs)
                    .ThenInclude(o => o.WasteType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
