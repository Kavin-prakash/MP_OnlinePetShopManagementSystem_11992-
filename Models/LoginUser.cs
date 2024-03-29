
using System.ComponentModel.DataAnnotations;

namespace OnlinePetShopManagementSystem.Models
{
    public class LoginUser
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
