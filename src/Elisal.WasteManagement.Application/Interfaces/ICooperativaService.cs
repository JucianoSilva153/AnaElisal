using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface ICooperativaService
{
    Task<TransactionDto> RegistarTransacaoAsync(TransactionDto dto);
    Task<IEnumerable<TransactionDto>> ObterHistoricoTransacoesAsync(int cooperativaId);
    Task<IEnumerable<TransactionDto>> ObterTodasTransacoesAsync(DateTime? inicio, DateTime? fim, int? coopId, int? wasteTypeId);
    Task<decimal> CalcularTotalValorizadoAsync(DateTime dataInicio, DateTime dataFim);
}
