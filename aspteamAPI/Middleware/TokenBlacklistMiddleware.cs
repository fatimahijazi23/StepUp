using aspteamAPI.IRepository;
using System.IdentityModel.Tokens.Jwt;

namespace aspteamAPI.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthRepositories authRepo)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jsonToken = tokenHandler.ReadJwtToken(token);
                    var jti = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (!string.IsNullOrEmpty(jti) && await authRepo.IsTokenBlacklistedAsync(jti))
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Token has been blacklisted");
                        return;
                    }
                }
                catch
                {
                    // Invalid token format - let the JWT middleware handle it
                }
            }

            await _next(context);
        }
    }
}