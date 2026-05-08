using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Interfaces;
using Elisal.WasteManagement.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Elisal.WasteManagement.Api.Hubs;

public class LocationHub : Hub
{
    private readonly IDriverLocationTracker _tracker;
    private readonly IServiceScopeFactory _scopeFactory;

    public LocationHub(IDriverLocationTracker tracker, IServiceScopeFactory scopeFactory)
    {
        _tracker = tracker;
        _scopeFactory = scopeFactory;
    }

    public async Task UpdateLocation(LocationUpdateDto location)
    {
        // 1. Actualizar posição em memória (singleton)
        _tracker.UpdatePosition(
            location.DriverId,
            location.RouteExecutionId,
            location.Latitude,
            location.Longitude);

        // 2. Broadcast para todos os clientes (dashboard, etc.)
        await Clients.All.SendAsync("LocationUpdated", location);

        // 3. Detectar conflito com outros motoristas (requer scope para serviços scoped)
        using var scope = _scopeFactory.CreateScope();
        var conflictService = scope.ServiceProvider.GetRequiredService<IRouteConflictService>();

        var conflict = await conflictService.CheckAndResolveConflictAsync(
            location.DriverId,
            location.RouteExecutionId,
            location.Latitude,
            location.Longitude);

        if (conflict != null)
        {
            // Notificar todos os clientes — ExecutarRota.razor filtra pelo executionId
            await Clients.All.SendAsync("PointMarkedInaccessible", new
            {
                conflict.AffectedExecutionId,
                conflict.CollectionPointId,
                conflict.PointName,
                Reason = "Outro motorista está a recolher neste ponto."
            });
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Remover o motorista do tracker quando a ligação SignalR cai
        // Nota: O DriverId não está disponível directamente aqui.
        // Para um mapeamento rigoroso ConnectionId → DriverId, seria necessário
        // um dicionário adicional. Por ora, a limpeza é feita pelo timeout de 10min.
        return base.OnDisconnectedAsync(exception);
    }
}
