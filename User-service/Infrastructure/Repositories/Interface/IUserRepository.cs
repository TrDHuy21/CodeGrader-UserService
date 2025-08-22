using Domain.Entities;

namespace Infrastructure.Repositories.Interface
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public Task<User> GetProfileByUserName(string username);


    }
}
