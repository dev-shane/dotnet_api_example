using dotnet_api_example.DatabaseContext;
using dotnet_api_example.Models;
using dotnet_api_example.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dotnet_api_example.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("auth")]
        public IActionResult Authenticate([FromBody] UserLoginInput Input, [FromServices] UserDbContext dbContext,[FromServices] ITokenServices tokenServices, [FromServices] IHttpContextAccessor httpContextAccessor)
        {
            var found = dbContext.Users.Where(r => r.UserAccount == Input.UserAccount && r.Password == Input.Password).SingleOrDefault();
            return Ok(new
            {
                AccessToken = tokenServices.GenerateAccessToken(new TokenContent
                {
                    UserAccount = found.UserAccount,
                    CreateTime = DateTime.Now,
                    ExpireTime = DateTime.Now.AddMinutes(5)
                }),
                UserAccount = found.UserAccount
            });
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterInput Input, [FromServices] UserDbContext dbContext)
        {
            if(dbContext.Users.Where(r => r.UserAccount == Input.UserAccount && r.Password == Input.Password).Any())
            {
                return BadRequest();
            }
            dbContext.Users.Add(new Models.User()
            {
                UserAccount = Input.UserAccount,
                Password = Input.Password,
                RegistrationDate = DateTime.Now
            });
            dbContext.SaveChanges();
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] UserRegisterInput Input, [FromServices] UserDbContext dbContext)
        {
            if (dbContext.Users.Where(r => r.UserAccount == Input.UserAccount && r.Password == Input.Password).Any())
            {
                return BadRequest();
            }
            dbContext.Users.Add(new Models.User()
            {
                UserAccount = Input.UserAccount,
                Password = Input.Password,
            });
            dbContext.SaveChanges();
            return Ok();
        }
    }
}
