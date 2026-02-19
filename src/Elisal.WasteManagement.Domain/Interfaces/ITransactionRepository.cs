using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByPeriodAsync(DateTime startDate, DateTime endDate, int? coopId = null);
}
