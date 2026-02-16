using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class OperationalAlertRepository : Repository<OperationalAlert>, IOperationalAlertRepository
{
    public OperationalAlertRepository(ElisalDbContext context) : base(context)
    {
    }
}
