using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.DTOs;

namespace Elisal.WasteManagement.Application.Interfaces;

public interface IOperationalAlertService
{
    Task<IEnumerable<OperationalAlertDto>> GetActiveAlertsAsync();
    Task MarkAsReadAsync(int id);
    Task ProcessAutomaticAlertsAsync();
}
