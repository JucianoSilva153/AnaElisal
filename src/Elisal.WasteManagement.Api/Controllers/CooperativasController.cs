using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;
using Elisal.WasteManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CooperativasController : ControllerBase
{
    private readonly ICooperativaService _cooperativaService;
    private readonly IRepository<Cooperative> _cooperativeRepository;
    private readonly IRepository<Transaction> _transactionRepository;

    public CooperativasController(
        ICooperativaService cooperativaService,
        IRepository<Cooperative> cooperativeRepository,
        IRepository<Transaction> transactionRepository)
    {
        _cooperativaService = cooperativaService;
        _cooperativeRepository = cooperativeRepository;
        _transactionRepository = transactionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var coops = await _cooperativeRepository.GetAllAsync();
        var dtos = coops.Select(c => c.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var coop = await _cooperativeRepository.GetByIdAsync(id);
        if (coop == null) return NotFound();

        return Ok(coop.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CooperativeDto dto)
    {
        var cooperative = new Cooperative
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Email = dto.Email,
            Address = dto.Address,
            AcceptedWasteTypes = dto.AcceptedWasteTypes
        };

        await _cooperativeRepository.AddAsync(cooperative);
        await _cooperativeRepository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cooperative.Id }, cooperative.ToDto());
    }

    [HttpPost("{id}/transacoes")]
    public async Task<IActionResult> RegistarTransacao(int id, [FromBody] TransactionDto dto)
    {
        if (id != dto.CooperativeId)
            return BadRequest(new { Message = "ID da cooperativa na URL diverge do corpo da requisição." });

        try
        {
            var result = await _cooperativaService.RegistarTransacaoAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao registar transação." });
        }
    }

    [HttpGet("{id}/historico")]
    public async Task<IActionResult> GetHistorico(int id)
    {
        var history = await _cooperativaService.ObterHistoricoTransacoesAsync(id);
        return Ok(history);
    }

    [HttpGet("transacoes")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetTodasTransacoes([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim,
        [FromQuery] int? coopId, [FromQuery] int? wasteId)
    {
        var result = await _cooperativaService.ObterTodasTransacoesAsync(inicio, fim, coopId, wasteId);
        return Ok(result);
    }

    [HttpPut("transacoes/{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AtualizarStatusTransacao(int id, [FromBody] UpdateTransactionStatusDto dto)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
                return NotFound(new { Message = "Transação não encontrada." });

            if (!Enum.TryParse<TransactionStatus>(dto.Status, true, out var newStatus))
                return BadRequest(new { Message = "Status inválido. Valores aceites: Pending, Completed, Cancelled." });

            transaction.Status = newStatus;
            await _transactionRepository.UpdateAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            return Ok(transaction.ToDto());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro ao atualizar status.", Details = ex.Message });
        }
    }
}

public class UpdateTransactionStatusDto
{
    public string Status { get; set; } = string.Empty;
}
