using dotnet_api_example.DatabaseContext;
using dotnet_api_example.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace dotnet_api_example.Security
{
    public class APIAuthenticationScheme
    {
        public const string AuthenticationSchemeName = "APIAuthenticationScheme";
        public const string AuthenticationDisplayName = "APIAuthenticationDisplayName";
    }
    public class APIAuthenticationHandler : IAuthenticationHandler
    {
        private AuthenticationScheme scheme;
        private HttpContext context;
        private UserDbContext dbContext;
        private ITokenServices tokenServices;

        public APIAuthenticationHandler(UserDbContext dbContext, ITokenServices tokenServices)
        {
            this.dbContext = dbContext;
            this.tokenServices = tokenServices;
        }
        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            try
            {
                String AccessToken = context.Request.Headers["Authorization"].ToString().Substring(7);
                if (String.IsNullOrEmpty(AccessToken))
                {
                    throw new UnauthorizedAccessException();
                }

                Boolean valid = tokenServices.ValidateAccessToken(AccessToken, out TokenContent content);
                if (!valid)
                {
                    throw new UnauthorizedAccessException();
                }

                var found = dbContext.Users.Where(r => r.UserAccount == content.UserAccount).SingleOrDefault();
                var identity = new System.Security.Claims.ClaimsIdentity(new List<Claim> {
                    new Claim(ClaimTypes.Name, found.UserAccount),
                    new Claim(ClaimTypes.Role, "User")
                });
                return AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(identity), this.scheme.Name));
            }
            catch(Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        public async Task ChallengeAsync(AuthenticationProperties properties)
        {
            context.Response.StatusCode = 401;
        }

        public async Task ForbidAsync(AuthenticationProperties properties)
        {
            context.Response.StatusCode = 404;
        }

        public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            this.context = context;
            this.scheme = scheme;
        }
    }
}
