using System.Linq;
using Elisal.WasteManagement.Application.DTOs;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Enums;

public static class DtoExtensions
{
    public static CollectionPointDto ToDto(this CollectionPoint collectionPoint)
    {
        if (collectionPoint == null) return null;
        return new CollectionPointDto
        {
            Id = collectionPoint.Id,
            Name = collectionPoint.Name,
            Address = collectionPoint.Address,
            Capacity = collectionPoint.Capacity,
            CurrentOccupancy = collectionPoint.CurrentOccupancy,
            IsActive = collectionPoint.IsActive,
            Latitude = collectionPoint.Latitude,
            Longitude = collectionPoint.Longitude,
            Municipality = collectionPoint.Municipality
        };
    }

    public static CollectionRecordDto ToDto(this CollectionRecord record)
    {
        if (record == null) return null;
        return new CollectionRecordDto
        {
            Id = record.Id,
            DateTime = record.DateTime,
            AmountKg = record.AmountKg,
            Notes = record.Notes,
            WasteTypeId = record.WasteTypeId,
            WasteTypeName = record.WasteType?.Name ?? string.Empty,
            CollectionPointId = record.CollectionPointId,
            CollectionPointName = record.CollectionPoint?.Name ?? string.Empty,
            UserId = record.UserId,
            UserName = record.User?.Name ?? string.Empty
        };
    }

    public static UserDto ToDto(this User user)
    {
        if (user == null) return null;
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    public static WasteTypeDto ToDto(this WasteType type)
    {
        if (type == null) return null;
        return new WasteTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description,
            ColorCode = type.ColorCode,
            IsRecyclable = type.IsRecyclable
        };
    }

    public static CooperativeDto ToDto(this Cooperative coop)
    {
        if (coop == null) return null;
        return new CooperativeDto
        {
            Id = coop.Id,
            Name = coop.Name,
            Contact = coop.Contact,
            Email = coop.Email,
            Address = coop.Address,
            AcceptedWasteTypes = coop.AcceptedWasteTypes
        };
    }

    public static RouteDto ToDto(this Elisal.WasteManagement.Domain.Entities.Route route)
    {
        if (route == null) return null;
        return new RouteDto
        {
            Id = route.Id,
            Name = route.Name,
            Description = route.Description,
            WeekDay = route.WeekDay,
            StartTime = route.StartTime,
            IsActive = route.IsActive,
            TotalDistance = route.TotalDistance,
            RoutePoints = route.RoutePoints?.Select(rp => rp.ToDto()).ToList() ?? new()
        };
    }

    public static RoutePointDto ToDto(this RoutePoint rp)
    {
        if (rp == null) return null;
        return new RoutePointDto
        {
            RouteId = rp.RouteId,
            CollectionPointId = rp.CollectionPointId,
            SequenceOrder = rp.SequenceOrder,
            CollectionPoint = rp.CollectionPoint?.ToDto()
        };
    }

    public static TransactionDto ToDto(this Transaction transaction)
    {
        if (transaction == null) return null;
        return new TransactionDto
        {
            Id = transaction.Id,
            DateTime = transaction.Date,
            CooperativeId = transaction.CooperativeId,
            WasteTypeId = transaction.WasteTypeId,
            AmountKg = transaction.AmountKg,
            TotalValue = transaction.Value,
            Status = transaction.Status,
            PricePerKg = transaction.AmountKg > 0 ? transaction.Value / (decimal)transaction.AmountKg : 0
        };
    }
}