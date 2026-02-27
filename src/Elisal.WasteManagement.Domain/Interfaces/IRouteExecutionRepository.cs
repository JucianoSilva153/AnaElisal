using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using System.Collections.Generic;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface IRouteExecutionRepository : IRepository<RouteExecution>
{
    Task<RouteExecution?> GetActiveByDriverIdAsync(int driverId);
}
