using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IWasteManagementService
{
    // Centros de Triagem
    Task<IEnumerable<SortingCenterDto>> GetAllCentrosAsync();
    Task<SortingCenterDto> CriarCentroAsync(CreateSortingCenterDto dto);
    Task<SortingCenterDto?> AtualizarCentroAsync(int id, CreateSortingCenterDto dto);

    // Recepções
    Task<WasteReceptionDto> RegistarRecepcaoAsync(CreateWasteReceptionDto dto);
    Task<IEnumerable<WasteReceptionDto>> GetRecepcoesPorPeriodoAsync(DateTime start, DateTime end);
    Task<IEnumerable<WasteReceptionDto>> GetRecepcoesPendentesAsync();
    Task<WasteReceptionDto?> GetRecepcaoByIdAsync(int id);
    Task<IEnumerable<RouteExecutionSummaryDto>> GetRotasAguardandoRecepcaoAsync(int? driverId = null);

    // Triagens
    Task<SortingBatchDto> IniciarTriagemAsync(CreateSortingBatchDto dto);
    Task<SortingBatchOutputDto> RegistarOutputTriagemAsync(CreateSortingBatchOutputDto dto);
    Task<SortingBatchDto> ConcluirTriagemAsync(int batchId);
    Task<IEnumerable<SortingBatchDto>> GetTriagensPorPeriodoAsync(DateTime start, DateTime end);
    Task<SortingBatchDto?> GetTriagemByIdAsync(int id);

    // Indicadores
    Task<EnvironmentalIndicatorsDto> GetIndicadoresAmbientaisAsync(DateTime start, DateTime end);
}
