using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface ICollectionPointRepository : IRepository<CollectionPoint>
{
    Task<IEnumerable<CollectionPoint>> GetByMunicipalityAsync(string municipality); //municipio
}
