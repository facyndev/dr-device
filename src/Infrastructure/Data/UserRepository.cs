using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext applicationDbcontext)
        : base(applicationDbcontext)
    {
    }

    public async Task<User?> GetByEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return await _dbSet.FirstOrDefaultAsync(user => user.Email == email);
    }
}
