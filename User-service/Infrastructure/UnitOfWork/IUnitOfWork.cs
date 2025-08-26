using Domain.Entities;
using Infrastructure.Repositories.Interface;

namespace Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepositories {get;}
        IRoleRepository RoleRepositories {get; }
        IRefreshTokenRepository RefreshTokenRepositories {get; }
        Task<int> SaveChangesAsync(); 

    }   
}
