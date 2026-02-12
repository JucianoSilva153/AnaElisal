using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface ICollectionRecordRepository : IRepository<CollectionRecord>
{
    Task<IEnumerable<CollectionRecord>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
}
