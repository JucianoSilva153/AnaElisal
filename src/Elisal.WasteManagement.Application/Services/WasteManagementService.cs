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

public class WasteManagementService : IWasteManagementService
{
    private readonly ISortingCenterRepository _centerRepo;
    private readonly IWasteReceptionRepository _receptionRepo;
    private readonly ISortingBatchRepository _batchRepo;
    private readonly IRepository<SortingBatchOutput> _outputRepo;
    private readonly IRepository<WasteType> _wasteTypeRepo;
    private readonly IRouteExecutionRepository _routeRepo;

    // Factores simplificados de CO₂ evitado por tonelada reciclada (por tipo de destino)
    private static readonly Dictionary<WasteDestinationType, double> Co2FactorPerTon = new()
    {
        { WasteDestinationType.Recycling, 1.2 },        // 1.2 ton CO₂ por ton reciclada
        { WasteDestinationType.Composting, 0.5 },        // 0.5 ton CO₂ por ton compostada
        { WasteDestinationType.InternalReuse, 0.8 },     // 0.8 ton CO₂ por ton reutilizada
        { WasteDestinationType.Landfill, 0.0 },          // Sem benefício
        { WasteDestinationType.Incineration, -0.1 },     // Ligeiro impacto negativo
        { WasteDestinationType.HazardousDisposal, 0.0 }  // Neutro
    };

    public WasteManagementService(
        ISortingCenterRepository centerRepo,
        IWasteReceptionRepository receptionRepo,
        ISortingBatchRepository batchRepo,
        IRepository<SortingBatchOutput> outputRepo,
        IRepository<WasteType> wasteTypeRepo,
        IRouteExecutionRepository routeRepo)
    {
        _centerRepo = centerRepo;
        _receptionRepo = receptionRepo;
        _batchRepo = batchRepo;
        _outputRepo = outputRepo;
        _wasteTypeRepo = wasteTypeRepo;
        _routeRepo = routeRepo;
    }

    #region Centros de Triagem

    public async Task<IEnumerable<SortingCenterDto>> GetAllCentrosAsync()
    {
        var centers = await _centerRepo.GetAllAsync();
        return centers.Select(MapCenterToDto);
    }

    public async Task<SortingCenterDto> CriarCentroAsync(CreateSortingCenterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Nome do centro é obrigatório.");

        var center = new SortingCenter
        {
            Name = dto.Name,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CapacityTonsPerDay = dto.CapacityTonsPerDay,
            Municipality = dto.Municipality,
            Contact = dto.Contact,
            IsActive = true
        };

        await _centerRepo.AddAsync(center);
        await _centerRepo.SaveChangesAsync();

        return MapCenterToDto(center);
    }

    public async Task<SortingCenterDto?> AtualizarCentroAsync(int id, CreateSortingCenterDto dto)
    {
        var center = await _centerRepo.GetByIdAsync(id);
        if (center == null) return null;

        center.Name = dto.Name;
        center.Address = dto.Address;
        center.Latitude = dto.Latitude;
        center.Longitude = dto.Longitude;
        center.CapacityTonsPerDay = dto.CapacityTonsPerDay;
        center.Municipality = dto.Municipality;
        center.Contact = dto.Contact;

        await _centerRepo.UpdateAsync(center);
        return MapCenterToDto(center);
    }

    #endregion

    #region Recepções

    public async Task<WasteReceptionDto> RegistarRecepcaoAsync(CreateWasteReceptionDto dto)
    {
        if (dto.GrossWeightKg <= 0)
            throw new ArgumentException("Peso bruto deve ser maior que zero.");
        if (dto.GrossWeightKg <= dto.TareWeightKg)
            throw new ArgumentException("Inconsistência de Pesagem: O peso bruto deve ser superior à tara para calcular o peso líquido.");

        var reception = new WasteReception
        {
            DateTime = DateTime.UtcNow,
            GrossWeightKg = dto.GrossWeightKg,
            TareWeightKg = dto.TareWeightKg,
            NetWeightKgStored = dto.GrossWeightKg - dto.TareWeightKg,
            RouteExecutionId = dto.RouteExecutionId,
            SortingCenterId = dto.SortingCenterId,
            ReceivedByUserId = dto.ReceivedByUserId,
            Notes = dto.Notes,
            IsSorted = false
        };

        await _receptionRepo.AddAsync(reception);
        await _receptionRepo.SaveChangesAsync();

        return MapReceptionToDto(reception);
    }

    public async Task<IEnumerable<WasteReceptionDto>> GetRecepcoesPorPeriodoAsync(DateTime start, DateTime end)
    {
        var receptions = await _receptionRepo.GetByPeriodAsync(start, end);
        return receptions.Select(MapReceptionToDto);
    }

    public async Task<IEnumerable<WasteReceptionDto>> GetRecepcoesPendentesAsync()
    {
        var receptions = await _receptionRepo.GetPendingSortingAsync();
        return receptions.Select(MapReceptionToDto);
    }

    public async Task<WasteReceptionDto?> GetRecepcaoByIdAsync(int id)
    {
        var reception = await _receptionRepo.GetByIdWithDetailsAsync(id);
        return reception == null ? null : MapReceptionToDto(reception);
    }

    public async Task<IEnumerable<RouteExecutionSummaryDto>> GetRotasAguardandoRecepcaoAsync(int? driverId = null)
    {
        var pendingRoutes = await _routeRepo.GetCompletedWithoutReceptionAsync(driverId);

        return pendingRoutes.Select(r => new RouteExecutionSummaryDto
        {
            Id = r.Id,
            RouteName = r.Route?.Name ?? $"Rota #{r.RouteId}",
            DriverName = r.Driver?.Name ?? $"Motorista #{r.DriverId}",
            EndTime = r.EndTime
        });
    }

    #endregion

    #region Triagens

    public async Task<SortingBatchDto> IniciarTriagemAsync(CreateSortingBatchDto dto)
    {
        var reception = await _receptionRepo.GetByIdAsync(dto.WasteReceptionId);
        if (reception == null)
            throw new ArgumentException("Recepção não encontrada.");

        // Validar se já existe uma triagem não cancelada para esta recepção
        var existingBatches = await _batchRepo.GetByReceptionIdAsync(dto.WasteReceptionId);
        if (existingBatches.Any(b => b.Status != SortingBatchStatus.Cancelled))
            throw new InvalidOperationException("Esta recepção já possui uma triagem associada (em progresso ou concluída).");

        var batch = new SortingBatch
        {
            WasteReceptionId = dto.WasteReceptionId,
            StartDateTime = DateTime.UtcNow,
            Status = SortingBatchStatus.InProgress,
            OperatorUserId = dto.OperatorUserId,
            Notes = dto.Notes
        };

        await _batchRepo.AddAsync(batch);
        await _batchRepo.SaveChangesAsync();

        return await MapBatchToDto(batch);
    }

    public async Task<SortingBatchOutputDto> RegistarOutputTriagemAsync(CreateSortingBatchOutputDto dto)
    {
        if (dto.WeightKg <= 0)
            throw new ArgumentException("Peso deve ser maior que zero.");

        var batch = await _batchRepo.GetByIdWithOutputsAsync(dto.SortingBatchId);
        if (batch == null)
            throw new ArgumentException("Lote de triagem não encontrado.");

        if (batch.Status == SortingBatchStatus.Completed)
            throw new InvalidOperationException("Triagem já foi concluída. Não é possível adicionar outputs.");

        // Validar que o peso total dos outputs não excede o peso da recepção
        var reception = await _receptionRepo.GetByIdAsync(batch.WasteReceptionId);
        if (reception != null)
        {
            var currentTotalWeight = batch.Outputs.Sum(o => o.WeightKg);
            if (currentTotalWeight + dto.WeightKg > reception.NetWeightKgStored * 1.05) // 5% tolerância
                throw new ArgumentException(
                    $"Peso total dos outputs ({currentTotalWeight + dto.WeightKg:N1} Kg) excede o peso da recepção ({reception.NetWeightKgStored:N1} Kg).");
        }

        var output = new SortingBatchOutput
        {
            SortingBatchId = dto.SortingBatchId,
            WasteTypeId = dto.WasteTypeId,
            WeightKg = dto.WeightKg,
            QualityGrade = dto.QualityGrade,
            DestinationType = dto.DestinationType,
            Notes = dto.Notes
        };

        await _outputRepo.AddAsync(output);
        await _outputRepo.SaveChangesAsync();

        return MapOutputToDto(output);
    }

    public async Task<SortingBatchDto> ConcluirTriagemAsync(int batchId)
    {
        var batch = await _batchRepo.GetByIdWithOutputsAsync(batchId);
        if (batch == null)
            throw new ArgumentException("Lote de triagem não encontrado.");

        if (batch.Status == SortingBatchStatus.Completed)
            throw new InvalidOperationException("Triagem já foi concluída.");

        batch.Status = SortingBatchStatus.Completed;
        batch.EndDateTime = DateTime.UtcNow;

        await _batchRepo.UpdateAsync(batch);

        // Marcar a recepção como triada
        var reception = await _receptionRepo.GetByIdAsync(batch.WasteReceptionId);
        if (reception != null)
        {
            reception.IsSorted = true;
            await _receptionRepo.UpdateAsync(reception);
        }

        return await MapBatchToDto(batch);
    }

    public async Task<IEnumerable<SortingBatchDto>> GetTriagensPorPeriodoAsync(DateTime start, DateTime end)
    {
        var batches = await _batchRepo.GetByPeriodAsync(start, end);
        var result = new List<SortingBatchDto>();
        foreach (var batch in batches)
        {
            result.Add(await MapBatchToDto(batch));
        }
        return result;
    }

    public async Task<SortingBatchDto?> GetTriagemByIdAsync(int id)
    {
        var batch = await _batchRepo.GetByIdWithOutputsAsync(id);
        return batch == null ? null : await MapBatchToDto(batch);
    }

    #endregion

    #region Indicadores Ambientais

    public async Task<EnvironmentalIndicatorsDto> GetIndicadoresAmbientaisAsync(DateTime start, DateTime end)
    {
        var receptions = (await _receptionRepo.GetByPeriodAsync(start, end)).ToList();
        var batches = (await _batchRepo.GetByPeriodAsync(start, end)).ToList();

        var allOutputs = batches
            .Where(b => b.Status == SortingBatchStatus.Completed)
            .SelectMany(b => b.Outputs)
            .ToList();

        var totalReceivedKg = receptions.Sum(r => r.NetWeightKgStored);
        var totalSortedKg = allOutputs.Sum(o => o.WeightKg);

        var recyclingKg = allOutputs.Where(o => o.DestinationType == WasteDestinationType.Recycling).Sum(o => o.WeightKg);
        var compostingKg = allOutputs.Where(o => o.DestinationType == WasteDestinationType.Composting).Sum(o => o.WeightKg);
        var landfillKg = allOutputs.Where(o => o.DestinationType == WasteDestinationType.Landfill).Sum(o => o.WeightKg);
        var incinerationKg = allOutputs.Where(o => o.DestinationType == WasteDestinationType.Incineration).Sum(o => o.WeightKg);
        var internalReuseKg = allOutputs.Where(o => o.DestinationType == WasteDestinationType.InternalReuse).Sum(o => o.WeightKg);
        var hazardousKg = allOutputs.Where(o => o.DestinationType == WasteDestinationType.HazardousDisposal).Sum(o => o.WeightKg);

        var landfillDiversionRate = totalSortedKg == 0 ? 0 : ((totalSortedKg - landfillKg) / totalSortedKg) * 100;
        var rejectedKg = allOutputs.Where(o => o.QualityGrade == QualityGrade.Rejected).Sum(o => o.WeightKg);
        var approvalRate = totalSortedKg == 0 ? 0 : ((totalSortedKg - rejectedKg) / totalSortedKg) * 100;

        // Calcular CO₂ evitado
        double co2Avoided = 0;
        foreach (var group in allOutputs.GroupBy(o => o.DestinationType))
        {
            if (Co2FactorPerTon.TryGetValue(group.Key, out var factor))
            {
                co2Avoided += (group.Sum(o => o.WeightKg) / 1000.0) * factor;
            }
        }

        var pendingReceptions = receptions.Count(r => !r.IsSorted);

        // Breakdown por destino
        var breakdown = new List<DestinationBreakdownDto>();
        if (totalSortedKg > 0)
        {
            AddBreakdown(breakdown, "Reciclagem", recyclingKg, totalSortedKg, "#10B981");
            AddBreakdown(breakdown, "Compostagem", compostingKg, totalSortedKg, "#84CC16");
            AddBreakdown(breakdown, "Aterro", landfillKg, totalSortedKg, "#EF4444");
            AddBreakdown(breakdown, "Incineração", incinerationKg, totalSortedKg, "#F97316");
            AddBreakdown(breakdown, "Reuso Interno", internalReuseKg, totalSortedKg, "#3B82F6");
            AddBreakdown(breakdown, "Perigosos", hazardousKg, totalSortedKg, "#8B5CF6");
        }

        // Tendência mensal (últimos 6 meses)
        var monthlyTrend = new List<ChartSeriesDto>();
        var current = new DateTime(start.Year, start.Month, 1);
        while (current <= end)
        {
            var monthEnd = current.AddMonths(1).AddDays(-1);
            var monthReceptions = receptions.Where(r => r.DateTime >= current && r.DateTime <= monthEnd);
            monthlyTrend.Add(new ChartSeriesDto
            {
                Label = current.ToString("MMM"),
                Value = Math.Round(monthReceptions.Sum(r => r.NetWeightKgStored) / 1000.0, 1)
            });
            current = current.AddMonths(1);
        }

        return new EnvironmentalIndicatorsDto
        {
            TotalReceivedTons = Math.Round(totalReceivedKg / 1000.0, 2),
            TotalSortedTons = Math.Round(totalSortedKg / 1000.0, 2),
            RecyclingTons = Math.Round(recyclingKg / 1000.0, 2),
            CompostingTons = Math.Round(compostingKg / 1000.0, 2),
            LandfillTons = Math.Round(landfillKg / 1000.0, 2),
            IncinerationTons = Math.Round(incinerationKg / 1000.0, 2),
            InternalReuseTons = Math.Round(internalReuseKg / 1000.0, 2),
            HazardousDisposalTons = Math.Round(hazardousKg / 1000.0, 2),
            LandfillDiversionRate = Math.Round(landfillDiversionRate, 1),
            Co2AvoidedTons = Math.Round(co2Avoided, 2),
            OverallApprovalRate = Math.Round(approvalRate, 1),
            PendingReceptions = pendingReceptions,
            DestinationBreakdown = breakdown,
            MonthlyTrend = monthlyTrend
        };
    }

    private void AddBreakdown(List<DestinationBreakdownDto> list, string name, double kg, double totalKg, string color)
    {
        if (kg > 0)
        {
            list.Add(new DestinationBreakdownDto
            {
                Destination = name,
                Percentage = Math.Round((kg / totalKg) * 100, 1),
                WeightTons = Math.Round(kg / 1000.0, 2),
                Color = color
            });
        }
    }

    #endregion

    #region Mappers

    private SortingCenterDto MapCenterToDto(SortingCenter c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address,
        Latitude = c.Latitude,
        Longitude = c.Longitude,
        CapacityTonsPerDay = c.CapacityTonsPerDay,
        Municipality = c.Municipality,
        Contact = c.Contact,
        IsActive = c.IsActive
    };

    private WasteReceptionDto MapReceptionToDto(WasteReception r) => new()
    {
        Id = r.Id,
        DateTime = r.DateTime,
        GrossWeightKg = r.GrossWeightKg,
        TareWeightKg = r.TareWeightKg,
        NetWeightKg = r.NetWeightKgStored,
        RouteExecutionId = r.RouteExecutionId,
        RouteExecutionName = r.RouteExecution?.Route?.Name,
        SortingCenterId = r.SortingCenterId,
        SortingCenterName = r.SortingCenter?.Name ?? string.Empty,
        ReceivedByUserId = r.ReceivedByUserId,
        ReceivedByUserName = r.ReceivedByUser?.Name ?? string.Empty,
        Notes = r.Notes,
        IsSorted = r.IsSorted
    };

    private async Task<SortingBatchDto> MapBatchToDto(SortingBatch b)
    {
        var reception = await _receptionRepo.GetByIdAsync(b.WasteReceptionId);
        var totalOutput = b.Outputs?.Sum(o => o.WeightKg) ?? 0;
        var rejectedWeight = b.Outputs?.Where(o => o.QualityGrade == QualityGrade.Rejected).Sum(o => o.WeightKg) ?? 0;

        return new SortingBatchDto
        {
            Id = b.Id,
            WasteReceptionId = b.WasteReceptionId,
            ReceptionNetWeightKg = reception?.NetWeightKgStored ?? 0,
            StartDateTime = b.StartDateTime,
            EndDateTime = b.EndDateTime,
            Status = b.Status,
            StatusText = GetStatusText(b.Status),
            OperatorUserId = b.OperatorUserId,
            OperatorName = b.Operator?.Name ?? string.Empty,
            Notes = b.Notes,
            TotalOutputWeightKg = totalOutput,
            ApprovalRate = totalOutput == 0 ? 0 : Math.Round(((totalOutput - rejectedWeight) / totalOutput) * 100, 1),
            Outputs = b.Outputs?.Select(MapOutputToDto).ToList() ?? new()
        };
    }

    private SortingBatchOutputDto MapOutputToDto(SortingBatchOutput o) => new()
    {
        Id = o.Id,
        SortingBatchId = o.SortingBatchId,
        WasteTypeId = o.WasteTypeId,
        WasteTypeName = o.WasteType?.Name ?? string.Empty,
        WasteTypeColor = o.WasteType?.ColorCode ?? "#64748B",
        WeightKg = o.WeightKg,
        QualityGrade = o.QualityGrade,
        QualityGradeText = GetQualityText(o.QualityGrade),
        DestinationType = o.DestinationType,
        DestinationTypeText = GetDestinationText(o.DestinationType),
        Notes = o.Notes
    };

    private static string GetStatusText(SortingBatchStatus status) => status switch
    {
        SortingBatchStatus.Pending => "Pendente",
        SortingBatchStatus.InProgress => "Em Progresso",
        SortingBatchStatus.Completed => "Concluída",
        SortingBatchStatus.Cancelled => "Cancelada",
        _ => "—"
    };

    private static string GetQualityText(QualityGrade grade) => grade switch
    {
        QualityGrade.A => "Alta (A)",
        QualityGrade.B => "Média (B)",
        QualityGrade.C => "Baixa (C)",
        QualityGrade.Rejected => "Rejeitado",
        _ => "—"
    };

    private static string GetDestinationText(WasteDestinationType dest) => dest switch
    {
        WasteDestinationType.Recycling => "Reciclagem",
        WasteDestinationType.Composting => "Compostagem",
        WasteDestinationType.Landfill => "Aterro Sanitário",
        WasteDestinationType.Incineration => "Incineração",
        WasteDestinationType.InternalReuse => "Reuso Interno",
        WasteDestinationType.HazardousDisposal => "Resíduos Perigosos",
        _ => "—"
    };

    #endregion
}
