using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Implement
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(USContext uSContext) : base(uSContext)
        {

        }

        public async Task<RefreshToken> GetByToken(string token)
        {
            return await _USContext.RefreshToken
                .Include(rf => rf.User)
               .FirstOrDefaultAsync(rf => rf.Token == token);
        }
    }
}
