using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;

namespace Elisal.WasteManagement.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ICollectionRecordRepository _collectionRecordRepository;
    private readonly IRepository<CollectionPoint> _collectionPointRepository;
    private readonly IRepository<WasteType> _wasteTypeRepository;
    private readonly IRepository<AuditLog> _auditLogRepository;
    private readonly IRepository<Route> _routeRepository;
    private readonly IRouteExecutionRepository _routeExecutionRepository;

    public DashboardService(
        ICollectionRecordRepository collectionRecordRepository,
        IRepository<CollectionPoint> collectionPointRepository,
        IRepository<WasteType> wasteTypeRepository,
        IRepository<AuditLog> auditLogRepository,
        IRepository<Route> routeRepository,
        IRouteExecutionRepository routeExecutionRepository)
    {
        _collectionRecordRepository = collectionRecordRepository;
        _collectionPointRepository = collectionPointRepository;
        _wasteTypeRepository = wasteTypeRepository;
        _auditLogRepository = auditLogRepository;
        _routeRepository = routeRepository;
        _routeExecutionRepository = routeExecutionRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(int? userId = null, string? role = null)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfLastMonth = startOfMonth.AddMonths(-1);
        var endOfLastMonth = startOfMonth.AddDays(-1);

        var allCurrentRecords = (await _collectionRecordRepository.GetByPeriodAsync(startOfMonth, now)).ToList();
        var allLastRecords = (await _collectionRecordRepository.GetByPeriodAsync(startOfLastMonth, endOfLastMonth))
            .ToList();

        // Filtering for Drivers/Operators
        var currentMonthRecords = (role == "Driver" && userId.HasValue)
            ? allCurrentRecords.Where(r => r.UserId == userId.Value).ToList()
            : allCurrentRecords;

        var lastMonthRecords = (role == "Driver" && userId.HasValue)
            ? allLastRecords.Where(r => r.UserId == userId.Value).ToList()
            : allLastRecords;

        var totalResiduos = currentMonthRecords.Sum(r => r.AmountKg) / 1000.0; // Ton
        var lastTotalResiduos = lastMonthRecords.Sum(r => r.AmountKg) / 1000.0;
        var variacaoResiduos =
            lastTotalResiduos == 0 ? 0 : ((totalResiduos - lastTotalResiduos) / lastTotalResiduos) * 100;

        var currentRecyclable = currentMonthRecords.Where(r => r.WasteType != null && r.WasteType.IsRecyclable)
            .Sum(r => r.AmountKg);
        var currentTotalKg = currentMonthRecords.Sum(r => r.AmountKg);
        var taxaReaproveitamento = currentTotalKg == 0 ? 0 : (currentRecyclable / currentTotalKg) * 100;

        var lastRecyclable = lastMonthRecords.Where(r => r.WasteType != null && r.WasteType.IsRecyclable)
            .Sum(r => r.AmountKg);
        var lastTotalKg = lastMonthRecords.Sum(r => r.AmountKg);
        var lastTaxa = lastTotalKg == 0 ? 0 : (lastRecyclable / lastTotalKg) * 100;
        var variacaoTaxa = taxaReaproveitamento - lastTaxa;

        var pontos = (await _collectionPointRepository.GetAllAsync()).ToList();
        var pontosAtivos = pontos.Count(p => p.IsActive);

        // Regra de Alertas Inteligentes: Pontos com > 90% de ocupação + Logs críticos
        var pontosCheios = pontos.Count(p => p.Capacity > 0 && (p.CurrentOccupancy / (double)p.Capacity) > 0.9);
        var logsCriticos = (await _auditLogRepository.GetAllAsync())
            .Count(l => l.Timestamp >= now.AddDays(-1) &&
                        (l.Action.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                         l.Details.Contains("Importante", StringComparison.OrdinalIgnoreCase)));

        var stats = new DashboardStatsDto
        {
            TotalResiduosMensal = Math.Round(totalResiduos, 1),
            VariacaoTotalResiduos = Math.Round(variacaoResiduos, 1),
            TaxaReaproveitamento = Math.Round(taxaReaproveitamento, 1),
            VariacaoTaxaReaproveitamento = Math.Round(variacaoTaxa, 1),
            PontosAtivos = pontosAtivos,
            AlertasOperacionais = pontosCheios + logsCriticos,
            PontosCriticosOcupacao = pontosCheios
        };

        // Lógica específica por papel
        if (role == "Driver" && userId.HasValue)
        {
            var driverExecutions = (await _routeExecutionRepository.GetAllAsync())
                .Where(e => e.DriverId == userId.Value);

            stats.MinhasRotasConcluidas = driverExecutions.Count(e => e.Status == RouteExecutionStatus.Completed);
            stats.MeuAproveitamentoKg = currentTotalKg;

            // Encontrar rota activa com progresso
            var activeExecution = await _routeExecutionRepository.GetActiveByDriverIdAsync(userId.Value);
            if (activeExecution != null)
            {
                stats.ProximaRota = new ActiveRoutePreviewDto
                {
                    Id = activeExecution.RouteId,
                    Nome = activeExecution.Route.Name,
                    Descricao = activeExecution.Route.Description,
                    TotalPontos = activeExecution.PointStatuses.Count,
                    PontosConcluidos = activeExecution.PointStatuses.Count(ps => ps.IsCompleted)
                };
            }

            // Impacto Pessoal
            foreach (var wt in await _wasteTypeRepository.GetAllAsync())
            {
                var weight = currentMonthRecords.Where(r => r.WasteTypeId == wt.Id).Sum(r => r.AmountKg);
                if (currentTotalKg > 0 && weight > 0)
                {
                    stats.ImpactoAmbientalPessoal.Add(new PieChartDto
                    {
                        Category = wt.Name,
                        Percentage = Math.Round((weight / currentTotalKg) * 100, 1),
                        Color = GetColorForWasteType(wt.Name)
                    });
                }
            }
        }
        else if (role == "Manager" || role == "Admin")
        {
            var activeExecutions = (await _routeExecutionRepository.GetAllAsync())
                .Where(e => e.Status == RouteExecutionStatus.InProgress);
            stats.RotasAtivasHoje = activeExecutions.Count();
        }

        // Charts data
        var last6Months = Enumerable.Range(0, 6).Select(i => startOfMonth.AddMonths(-i)).Reverse();
        foreach (var month in last6Months)
        {
            var endOfMonth = month.AddMonths(1).AddDays(-1);
            var monthRecords = await _collectionRecordRepository.GetByPeriodAsync(month, endOfMonth);

            var filteredMonthRecords = (role == "Driver" && userId.HasValue)
                ? monthRecords.Where(r => r.UserId == userId.Value)
                : monthRecords;

            stats.VolumeMensal.Add(new ChartSeriesDto
            {
                Label = month.ToString("MMM"),
                Value = Math.Round(filteredMonthRecords.Sum(r => r.AmountKg) / 1000.0, 1)
            });
        }

        var wasteTypes = await _wasteTypeRepository.GetAllAsync();
        foreach (var wt in wasteTypes)
        {
            var wtWeight = currentMonthRecords.Where(r => r.WasteTypeId == wt.Id).Sum(r => r.AmountKg);
            if (currentTotalKg > 0)
            {
                stats.DistribuicaoPorTipo.Add(new PieChartDto
                {
                    Category = wt.Name,
                    Percentage = Math.Round((wtWeight / currentTotalKg) * 100, 1),
                    Color = GetColorForWasteType(wt.Name)
                });
            }
        }

        // Recent Collections
        var allRecordsQuery = (await _collectionRecordRepository.GetAllAsync());

        var filteredRecords = (role == "Driver" && userId.HasValue)
            ? allRecordsQuery.Where(r => r.UserId == userId.Value)
            : allRecordsQuery;

        var finalRecords = filteredRecords
            .OrderByDescending(r => r.DateTime)
            .Take(5);

        foreach (var r in finalRecords)
        {
            stats.ColetasRecentes.Add(new RecentCollectionDto
            {
                DataHora = r.DateTime.ToString("dd MMM, HH:mm"),
                Localizacao = r.CollectionPoint?.Name ?? "N/A",
                Tipo = r.WasteType?.Name ?? "N/A",
                Peso = $"{r.AmountKg / 1000.0:F1} Ton",
                Status = "Concluído"
            });
        }

        return stats;
    }

    private string GetColorForWasteType(string name)
    {
        return name.ToLower() switch
        {
            "orgânico" => "#2f7f34",
            "plástico" => "#F97316",
            "papel" => "#EAB308",
            "vidro" => "#10B981",
            _ => "#64748B"
        };
    }
}
