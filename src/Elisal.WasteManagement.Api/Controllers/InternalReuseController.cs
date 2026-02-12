using Microsoft.AspNetCore.Mvc;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/reaproveitamento-interno")]
public class InternalReuseController : ControllerBase
{
    private readonly IRepository<Cooperative> _cooperativeRepository;

    public InternalReuseController(IRepository<Cooperative> cooperativeRepository)
    {
        _cooperativeRepository = cooperativeRepository;
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        // Find the internal reuse cooperative
        // We look for name or ID 999
        var coops = await _cooperativeRepository.GetAllAsync();
        var internalCoop = coops.FirstOrDefault(c => c.Id == 999 || c.Name.Contains("Elisal - Reaproveitamento Interno"));

        if (internalCoop == null)
        {
            return NotFound("Cooperativa de sistema não encontrada.");
        }

        return Ok(new { InternalCooperativeId = internalCoop.Id });
    }
}
