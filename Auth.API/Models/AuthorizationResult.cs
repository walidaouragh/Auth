using System.Collections.Generic;

namespace Auth.API.Models
{
    public class AuthorizationResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}