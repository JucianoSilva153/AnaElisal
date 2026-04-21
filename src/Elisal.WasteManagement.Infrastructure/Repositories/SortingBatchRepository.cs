using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class SortingBatchRepository : Repository<SortingBatch>, ISortingBatchRepository
{
    public SortingBatchRepository(ElisalDbContext context) : base(context) { }

    public async Task<IEnumerable<SortingBatch>> GetByPeriodAsync(DateTime start, DateTime end)
    {
        return await _context.SortingBatches
            .Include(b => b.Operator)
            .Include(b => b.WasteReception)
            .Include(b => b.Outputs)
                .ThenInclude(o => o.WasteType)
            .Where(b => b.StartDateTime >= start && b.StartDateTime <= end)
            .OrderByDescending(b => b.StartDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<SortingBatch>> GetByStatusAsync(SortingBatchStatus status)
    {
        return await _context.SortingBatches
            .Include(b => b.Operator)
            .Include(b => b.WasteReception)
            .Include(b => b.Outputs)
                .ThenInclude(o => o.WasteType)
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.StartDateTime)
            .ToListAsync();
    }

    public async Task<SortingBatch?> GetByIdWithOutputsAsync(int id)
    {
        return await _context.SortingBatches
            .Include(b => b.Operator)
            .Include(b => b.WasteReception)
            .Include(b => b.Outputs)
                .ThenInclude(o => o.WasteType)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<SortingBatch>> GetByReceptionIdAsync(int receptionId)
    {
        return await _context.SortingBatches
            .Include(b => b.Operator)
            .Include(b => b.Outputs)
                .ThenInclude(o => o.WasteType)
            .Where(b => b.WasteReceptionId == receptionId)
            .OrderByDescending(b => b.StartDateTime)
            .ToListAsync();
    }
}
