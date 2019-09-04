using Auth.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.DbContext
{
    public class AuthDbContext : IdentityDbContext
    {
        public DbSet<User> Users { get; set; }
        
        public AuthDbContext()
        {
            
        }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
            
        }
    }
}