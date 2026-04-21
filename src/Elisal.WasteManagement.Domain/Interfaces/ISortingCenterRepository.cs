using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface ISortingCenterRepository : IRepository<SortingCenter>
{
    Task<IEnumerable<SortingCenter>> GetActiveAsync();
}
