using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ElisalDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate, int? coopId = null)
    {
        var query = _dbSet
            .Include(t => t.Cooperative)
            .Include(t => t.WasteType)
            .Where(t => t.Date >= startDate && t.Date <= endDate);

        if (coopId.HasValue && coopId.Value > 0)
        {
            query = query.Where(t => t.CooperativeId == coopId.Value);
        }

        return await query.ToListAsync();
    }
}
