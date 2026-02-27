using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

public class CooperativaService : ICooperativaService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Cooperative> _cooperativeRepository;

    public CooperativaService(IRepository<Transaction> transactionRepository,
        IRepository<Cooperative> cooperativeRepository)
    {
        _transactionRepository = transactionRepository;
        _cooperativeRepository = cooperativeRepository;
    }

    public async Task<TransactionDto> RegistarTransacaoAsync(TransactionDto dto)
    {
        if (dto.AmountKg <= 0)
            throw new ArgumentException("Quantidade inválida.");

        if (dto.TotalValue <= 0)
            throw new ArgumentException("Valor deve ser positivo.");

        var transacao = new Transaction
        {
            Date = DateTime.UtcNow,
            CooperativeId = dto.CooperativeId,
            WasteTypeId = dto.WasteTypeId,
            AmountKg = dto.AmountKg,
            Value = dto.TotalValue,
            Status = dto.Status
        };

        await _transactionRepository.AddAsync(transacao);
        await _transactionRepository.SaveChangesAsync();

        return transacao.ToDto();
    }

    public async Task<IEnumerable<TransactionDto>> ObterHistoricoTransacoesAsync(int cooperativaId)
    {
        var all = await _transactionRepository.GetAllAsync();
        // Filtering in memory again due to generic repository limitation (usually needs Specification pattern or specialized repo)
        return all.Where(t => t.CooperativeId == cooperativaId).OrderByDescending(t => t.Date).Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TransactionDto>> ObterTodasTransacoesAsync(DateTime? inicio, DateTime? fim,
        int? coopId, int? wasteTypeId)
    {
        var all = await _transactionRepository.GetAllAsync();
        var query = all.AsQueryable();

        if (inicio.HasValue) query = query.Where(t => t.Date >= inicio.Value.Date);
        if (fim.HasValue) query = query.Where(t => t.Date <= fim.Value.Date.AddDays(1).AddTicks(-1));
        if (coopId.HasValue) query = query.Where(t => t.CooperativeId == coopId.Value);
        if (wasteTypeId.HasValue) query = query.Where(t => t.WasteTypeId == wasteTypeId.Value);

        return query.OrderByDescending(t => t.Date).Select(t => t.ToDto()).ToList();
    }

    public async Task<decimal> CalcularTotalValorizadoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var all = await _transactionRepository.GetAllAsync();
        return all
            .Where(t => t.Date >= dataInicio && t.Date <= dataFim && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Value);
    }
}
