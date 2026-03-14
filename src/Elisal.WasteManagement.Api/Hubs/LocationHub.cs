using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Elisal.WasteManagement.Api.Hubs;

public class LocationHub : Hub
{
    public async Task UpdateLocation(LocationUpdateDto location)
    {
        // Broadcast the location update to all connected clients (e.g., dashboard managers)
        // We could use groups to only send to specific users, but for now we broadcast to "LocationUpdated" listeners
        await Clients.All.SendAsync("LocationUpdated", location);
    }
}
