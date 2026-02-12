using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class CollectionPointRepository : Repository<CollectionPoint>, ICollectionPointRepository
{
    public CollectionPointRepository(ElisalDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CollectionPoint>> GetByMunicipalityAsync(string municipality)
    {
        return await _dbSet
            .Where(p => p.Municipality == municipality)
            .ToListAsync();
    }
}
