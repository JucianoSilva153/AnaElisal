using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface ISortingBatchRepository : IRepository<SortingBatch>
{
    Task<IEnumerable<SortingBatch>> GetByPeriodAsync(DateTime start, DateTime end);
    Task<IEnumerable<SortingBatch>> GetByStatusAsync(SortingBatchStatus status);
    Task<SortingBatch?> GetByIdWithOutputsAsync(int id);
    Task<IEnumerable<SortingBatch>> GetByReceptionIdAsync(int receptionId);
}
