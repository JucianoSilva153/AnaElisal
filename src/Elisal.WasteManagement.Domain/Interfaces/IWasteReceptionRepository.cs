using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface IWasteReceptionRepository : IRepository<WasteReception>
{
    Task<IEnumerable<WasteReception>> GetByPeriodAsync(DateTime start, DateTime end);
    Task<IEnumerable<WasteReception>> GetBySortingCenterAsync(int sortingCenterId);
    Task<IEnumerable<WasteReception>> GetPendingSortingAsync();
    Task<WasteReception?> GetByIdWithDetailsAsync(int id);
}
