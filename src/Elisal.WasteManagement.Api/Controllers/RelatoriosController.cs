using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;

    public RelatoriosController(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    [HttpGet("pdf/{tipo}")]
    public async Task<IActionResult> GerarPdf(
        string tipo,
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim,
        [FromQuery] int? municipioId,
        [FromQuery] int? residuoId,
        [FromQuery] int? coopId)
    {
        try
        {
            var filtros = new FiltrosRelatorio
            {
                MunicipioId = municipioId,
                TipoResiduoId = residuoId,
                CooperativaId = coopId
            };

            var pdfBytes = await _relatorioService.GerarRelatorioPdfAsync(tipo, inicio, fim, filtros);
            return File(pdfBytes, "application/pdf", $"relatorio_{tipo}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao gerar relatório PDF", Details = ex.Message });
        }
    }

    [HttpGet("excel/{tipo}")]
    public async Task<IActionResult> GerarExcel(
        string tipo,
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim,
        [FromQuery] int? municipioId,
        [FromQuery] int? residuoId,
        [FromQuery] int? coopId)
    {
        try
        {
            var filtros = new FiltrosRelatorio
            {
                MunicipioId = municipioId,
                TipoResiduoId = residuoId,
                CooperativaId = coopId
            };

            var excelBytes = await _relatorioService.GerarRelatorioExcelAsync(tipo, inicio, fim, filtros);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"relatorio_{tipo}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao gerar relatório Excel", Details = ex.Message });
        }
    }
}
