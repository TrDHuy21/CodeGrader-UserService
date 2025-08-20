using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implement
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(USContext uSContext) : base(uSContext)
        {
            
        }
        public async Task<User> GetProfileByUserName(string username)
        {
            return await _USContext.User.FirstOrDefaultAsync(us => us.Username == username);
        }
    }
}
