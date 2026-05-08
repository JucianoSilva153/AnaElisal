using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface IRoutePointExecutionStatusRepository : IRepository<RoutePointExecutionStatus>
{
    Task<IEnumerable<RoutePointExecutionStatus>> GetPendingByExecutionIdAsync(int executionId);
    Task<bool> HasAnyPendingAtPointAsync(int executionId, int collectionPointId);
}
