using Infrastructure.Repositories.Interface;
using Infrastructure.UnitOfWork;

namespace WebApi.Midleware
{
    public class TokenValidationMidleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMidleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUnitOfWork unitOfWork)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var useIdClaim = context.User.FindFirst("Id")?.Value;
                var iatClaim = context.User.FindFirst("iat")?.Value;

                if (!string.IsNullOrEmpty(useIdClaim) && !string.IsNullOrEmpty(iatClaim))
                {
                    var userId = int.Parse(useIdClaim);
                    var tokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatClaim)).UtcDateTime;

                    var user = await unitOfWork.UserRepositories.GetByIdAsync(userId);
                    if (user != null && user.PasswordChangedAt.HasValue && tokenIssuedAt < user.PasswordChangedAt.Value)
                    {
                        context.Response.StatusCode = 401; // Unauthorized
                        await context.Response.WriteAsync("Token is no longer valid.");
                        return;
                    }               
                }
            }
           await _next(context);
        }

    }
}
