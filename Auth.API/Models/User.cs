using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models
{
    public class User : IdentityUser
    {
        [Column] public string FullName { get; set; }
    }
}