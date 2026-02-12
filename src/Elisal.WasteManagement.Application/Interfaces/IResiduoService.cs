using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IResiduoService
{
    Task<CollectionRecordDto> RegistarRecolhaAsync(CreateCollectionRecordDto dto);
    Task<IEnumerable<CollectionRecordDto>> ObterEstatisticasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<double> ObterTotalPorTipoAsync(int tipoResiduoId, DateTime dataInicio, DateTime dataFim);
    Task<double> CalcularTaxaReciclagemAsync(DateTime dataInicio, DateTime dataFim);
}
