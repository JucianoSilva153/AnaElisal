using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class CollectionRecordRepository : Repository<CollectionRecord>, ICollectionRecordRepository
{
    public CollectionRecordRepository(ElisalDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CollectionRecord>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(r => r.DateTime >= startDate && r.DateTime <= endDate.Date)
            .Include(r => r.WasteType)
            .Include(r => r.CollectionPoint)
            .Include(r => r.User)
            .ToListAsync();
    }
}
