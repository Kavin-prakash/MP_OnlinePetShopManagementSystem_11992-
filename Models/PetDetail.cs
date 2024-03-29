using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlinePetShopManagementSystem.Models
{
    public class PetDetail
    {
        [Key]
        public int Id { get; set; }

        
        public string? PetName { get; set; }

       
        public string? PetCategory { get; set; }

        
        public string? PetBreed { get; set; }
     
        public long? PetPrice { get; set; }

        [NotMapped]
        public IFormFile? PetImage { get; set; }

        
        public string? PetDescription { get; set; }

      

        public int? PetStock { get; set; }


        public string? UniqueFileName { get; set; }

       
       
       // public ICollection<Appointment> Appointments { get; }

       

    }
    
}
