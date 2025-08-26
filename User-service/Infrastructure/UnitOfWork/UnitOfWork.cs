using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repositories.Implement;
using Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly USContext _context;

        public UnitOfWork(USContext context)
        {
            _context = context;
            UserRepositories = new UserRepository(_context);
            RoleRepositories = new RoleRepository(_context);
            RefreshTokenRepositories = new RefreshTokenRepository(_context);
        }

        public IUserRepository UserRepositories { get; }
        public IRoleRepository RoleRepositories { get; }
        public IRefreshTokenRepository RefreshTokenRepositories { get; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
