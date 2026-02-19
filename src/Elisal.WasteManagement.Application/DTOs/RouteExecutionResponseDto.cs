using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Application.DTOs;

public class RouteExecutionResponseDto
{
    public int ExecutionId { get; set; }
    public RouteExecutionDto Status { get; set; } = null!;
}
