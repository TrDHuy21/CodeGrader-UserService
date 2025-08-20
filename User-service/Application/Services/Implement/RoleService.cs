using Application.Services.Interface;
using Domain.Entities;
using Infrastructure.UnitOfWork;

namespace Application.Services.Implement
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task AddRoleAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRoleAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Role> GetRoleByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateRoleAsync(Role role)
        {
            throw new NotImplementedException();
        }
    }
}
