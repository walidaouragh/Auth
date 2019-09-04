using System.Threading.Tasks;
using Auth.API.Models;
using Auth.API.Repositories;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService, IAuthRepository authRepository)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserToPost userToPost)
        {
            var result = await _authService.RegisterUser(userToPost);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserToLogin userToLogin)
        {
            var result = await _authService.LoginUser(userToLogin.Email, userToLogin.Password);
            if (!result.Success)
            {
                return Unauthorized("User not found or password is invalid");
            }

            return Ok(result);
        }
    }
}