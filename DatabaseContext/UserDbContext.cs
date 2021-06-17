using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_api_example.DatabaseContext
{
    public partial class UserDbContext:DbContext
    {
        public UserDbContext()
        {
        }

        public UserDbContext(DbContextOptions<UserDbContext> options): base(options)
        {
        }
        public virtual DbSet<Models.User> Users { get; set; }
    }
}
