using Elisal.WasteManagement.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;

namespace Elisal.WasteManagement.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
}
