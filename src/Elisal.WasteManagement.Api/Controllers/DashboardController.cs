using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elisal.WasteManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? userId = int.TryParse(userIdStr, out var id) ? id : null;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        var stats = await _dashboardService.GetStatsAsync(userId, role);
        return Ok(stats);
    }
}
