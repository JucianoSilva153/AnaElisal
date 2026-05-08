using System.Threading.Tasks;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IRouteConflictService
{
    /// <summary>
    /// Verifica se o motorista está perto de um ponto que já tem outro
    /// motorista activo a recolher. Se sim, marca o ponto como inacessível
    /// na execução do motorista e retorna o resultado do conflito.
    /// </summary>
    Task<PointConflictResult?> CheckAndResolveConflictAsync(
        int driverId, int executionId, double driverLat, double driverLon);
}

public record PointConflictResult(
    int CollectionPointId,
    string PointName,
    int ConflictingDriverId,
    int AffectedExecutionId);
