using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Auth.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Auth.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager,
               IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterUser(UserToPost userToPost)
        {
            var user = new User()
            {
                UserName = userToPost.UserName,
                Email = userToPost.Email,
                FullName = userToPost.FullName
            };

            var response = await _userManager.CreateAsync(user, userToPost.Password);

            return response;
        }

        public async Task<AuthorizationResult> LoginUser(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AuthorizationResult()
                {
                    Errors = new [] {"User does not exist"} 
                };
            }

            var userHasPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!userHasPassword)
            {
                return new AuthorizationResult()
                {
                    Errors = new [] {"User/password invalid"} 
                };
            }

            return GenerateJwtToken(user);
        }

        private AuthorizationResult GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AuthConfig:development:secret")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(30));

            var token = new JwtSecurityToken(
                _configuration.GetValue<string>("AuthConfig:development:JwtIssuer"),
                _configuration.GetValue<string>("AuthConfig:development:JwtIssuer"),
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthorizationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }

        //this method works too!
        /*private AuthorizationResult GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("AuthConfig:development:secret"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("id", user.Id),
                }),
                Expires = DateTime.Now.AddDays(Convert.ToDouble(30)),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthorizationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }*/
    }
}