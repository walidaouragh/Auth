using System.Threading.Tasks;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUser(UserToPost userToPost);

        Task<AuthorizationResult> LoginUser(string email, string password);
    }
}