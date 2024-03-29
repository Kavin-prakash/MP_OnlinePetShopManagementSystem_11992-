using System.ComponentModel.DataAnnotations;

namespace OnlinePetShopManagementSystem.Models
{
    public class LoginAdmin
    {
        [Key]
        public int AdminUserId { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }


       // public List<Appointment> Appointments { get; set;}
    }
}

