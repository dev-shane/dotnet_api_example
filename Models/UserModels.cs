using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_api_example.Models
{
    public partial class User
    {
        public virtual int UserId { get; set; }
        public virtual String UserAccount { get; set; }
        public virtual String Password { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class UserRegisterInput: User
    {
        [JsonIgnore]
        public override int UserId { get; set; }
        public override String UserAccount { get; set; }
        public override String Password { get; set; }
    }

    public class UserLoginInput : User
    {
        [JsonIgnore]
        public override int UserId { get; set; }
        public override String UserAccount { get; set; }
        public override String Password { get; set; }
    }

    public class TokenContent
    {
        public String UserAccount { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ExpireTime { get; set; }
    }
    public class AppConfig
    {
        public String JwtSercet { get; set; }
        public String RefreshTokenSecret { get; set; }
    }
}
