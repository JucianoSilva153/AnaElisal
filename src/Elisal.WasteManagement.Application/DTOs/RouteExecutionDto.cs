using Elisal.WasteManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elisal.WasteManagement.Application.DTOs
{
    public class RouteExecutionDto
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int DriverId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public RouteExecutionStatus Status { get; set; } = RouteExecutionStatus.InProgress;

        public IEnumerable<RoutePointExecutionStatusDto> PointStatuses { get; set; } = new List<RoutePointExecutionStatusDto>();
    }

    public class RoutePointExecutionStatusDto
    {
        public int Id { get; set; }
        public int RouteExecutionId { get; set; }
        public int CollectionPointId { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }
    }

    public class CreateRouteExecutionDto
    {

    }
}
