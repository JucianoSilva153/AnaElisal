using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class SortingCenterRepository : Repository<SortingCenter>, ISortingCenterRepository
{
    public SortingCenterRepository(ElisalDbContext context) : base(context) { }

    public async Task<IEnumerable<SortingCenter>> GetActiveAsync()
    {
        return await _context.SortingCenters
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
