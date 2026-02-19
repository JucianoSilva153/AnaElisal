using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

public class ResiduoService : IResiduoService
{
    private readonly ICollectionRecordRepository _collectionRecordRepository;
    private readonly IRepository<WasteType> _wasteTypeRepository;
    private readonly IRepository<AuditLog> _auditLogRepository;

    public ResiduoService(
        ICollectionRecordRepository collectionRecordRepository,
        IRepository<WasteType> wasteTypeRepository,
        IRepository<AuditLog> auditLogRepository)
    {
        _collectionRecordRepository = collectionRecordRepository;
        _wasteTypeRepository = wasteTypeRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<CollectionRecordDto> RegistarRecolhaAsync(CreateCollectionRecordDto dto)
    {
        if (dto.AmountKg <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");

        var registoRecolha = new CollectionRecord
        {
            DateTime = dto.DateTime == default ? DateTime.UtcNow : dto.DateTime,
            AmountKg = dto.AmountKg,
            Notes = dto.Notes,
            WasteTypeId = dto.WasteTypeIds?.FirstOrDefault() ?? dto.WasteTypeId,
            CollectionPointId = dto.CollectionPointId,
            UserId = dto.UserId,
            RecordWasteTypes =
                dto.WasteTypeIds?.Select(id => new CollectionRecordWasteType { WasteTypeId = id }).ToList() ??
                new List<CollectionRecordWasteType>()
        };

        await _collectionRecordRepository.AddAsync(registoRecolha);
        await _collectionRecordRepository.SaveChangesAsync();

        // Audit Log manually or via aspect
        return registoRecolha.ToDto();
    }

    public async Task<IEnumerable<CollectionRecordDto>> ObterEstatisticasPorPeriodoAsync(DateTime dataInicio,
        DateTime dataFim)
    {
        var records = await _collectionRecordRepository.GetByPeriodAsync(dataInicio, dataFim);
        return records.Select(r => r.ToDto());
    }

    public async Task<double> ObterTotalPorTipoAsync(int tipoResiduoId, DateTime dataInicio, DateTime dataFim)
    {
        var records = await _collectionRecordRepository.GetByPeriodAsync(dataInicio, dataFim);
        return records.Where(r => r.WasteTypeId == tipoResiduoId).Sum(r => r.AmountKg);
    }

    public async Task<double> CalcularTaxaReciclagemAsync(DateTime dataInicio, DateTime dataFim)
    {
        var records = await _collectionRecordRepository.GetByPeriodAsync(dataInicio, dataFim);
        if (!records.Any()) return 0;

        var totalWeight = records.Sum(r => r.AmountKg);
        var recyclableWeight = records.Where(r => r.WasteType != null && r.WasteType.IsRecyclable).Sum(r => r.AmountKg);

        if (totalWeight == 0) return 0;

        return (recyclableWeight / totalWeight) * 100;
    }
}
