using dotnet_api_example.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_api_example.Security
{
    public interface ITokenServices
    {
        public abstract String GenerateAccessToken(TokenContent content);
        public abstract Boolean ValidateAccessToken(String input, out TokenContent content);
    }
    public class TokenServices: ITokenServices
    {
        private IOptions<AppConfig> appConfig;
        private IHttpContextAccessor httpContextAccessor;
        public TokenServices(IOptions<AppConfig> appConfig, IHttpContextAccessor httpContextAccessor)
        {
            this.appConfig = appConfig;
            this.httpContextAccessor = httpContextAccessor;
        }
        public String GenerateAccessToken(TokenContent content)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appConfig.Value.JwtSercet));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, content.UserAccount),
                new Claim(JwtRegisteredClaimNames.AuthTime, ((DateTimeOffset)content.CreateTime).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)content.ExpireTime).ToUnixTimeSeconds().ToString()),
            };
            var token = new JwtSecurityToken(
                issuer: $"https://{this.httpContextAccessor.HttpContext.Request.Host.Value}",
                audience: $"https://{this.httpContextAccessor.HttpContext.Request.Host.Value}",
                claims: claims,
                notBefore: content.CreateTime,
                expires: content.ExpireTime,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public Boolean ValidateAccessToken(String input, out TokenContent content)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = $"https://{this.httpContextAccessor.HttpContext.Request.Host.Value}",
                ValidAudience = $"https://{this.httpContextAccessor.HttpContext.Request.Host.Value}",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.appConfig.Value.JwtSercet))
            };
            try
            {
                SecurityToken validatedToken;
                tokenHandler.ValidateToken(input, validationParameters, out validatedToken);
                if (validatedToken != null)
                {
                    var JwtSecurityValidatedToken = (JwtSecurityToken)validatedToken;

                    content = new TokenContent()
                    {
                        UserAccount = JwtSecurityValidatedToken.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault().Value,
                        CreateTime = new DateTime(long.Parse(JwtSecurityValidatedToken.Claims.Where(c => c.Type == JwtRegisteredClaimNames.AuthTime).FirstOrDefault().Value)),
                        ExpireTime = new DateTime(long.Parse(JwtSecurityValidatedToken.Claims.Where(c => c.Type == JwtRegisteredClaimNames.AuthTime).FirstOrDefault().Value)),

                    };
                    return true;
                }
                throw new NullReferenceException();
            }
            catch (Exception)
            {
                content = null;
                return false;
            }
        }
    }
}
