using System;
using System.Collections.Generic;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IDriverLocationTracker
{
    void UpdatePosition(int driverId, int executionId, double lat, double lon);
    void RemoveDriver(int driverId);
    IReadOnlyList<ActiveDriverPosition> GetAll();
}

public record ActiveDriverPosition(
    int DriverId,
    int ExecutionId,
    double Lat,
    double Lon,
    DateTime LastSeen);
