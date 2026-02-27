using System.Threading.Tasks;
using Elisal.WasteManagement.Domain.Entities;
using Elisal.WasteManagement.Domain.Interfaces;
using Elisal.WasteManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ElisalDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }
}
