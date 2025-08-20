using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repositories.Interface;

namespace Infrastructure.Repositories.Implement
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(USContext uSContext) : base(uSContext)
        {
        }
    }
}
