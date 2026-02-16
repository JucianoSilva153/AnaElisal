using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elisal.WasteManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OperationalAlertsController : ControllerBase
{
    private readonly IOperationalAlertService _alertService;

    public OperationalAlertsController(IOperationalAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OperationalAlertDto>>> GetActiveAlerts()
    {
        var alerts = await _alertService.GetActiveAlertsAsync();
        return Ok(alerts);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _alertService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPost("process")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ProcessAlerts()
    {
        await _alertService.ProcessAutomaticAlertsAsync();
        return Ok(new { message = "Alertas processados com sucesso." });
    }
}
