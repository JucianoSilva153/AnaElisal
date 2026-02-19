using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int? userId = null, string? role = null);
}
