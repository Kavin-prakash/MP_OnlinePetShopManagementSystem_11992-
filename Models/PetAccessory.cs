using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;


namespace OnlinePetShopManagementSystem.Models
{
    public class PetAccessory
    {
        [Key]
        public int Id { get; set; }

        
        public string? PetAccesoryName { get; set; }


        public string? PetAccessoryPrice { get; set; }


        [NotMapped]
        public IFormFile? AccessoryImage { get; set; }

     
        public string? PetAccessoryDescription { get; set; }


        public string? UniqueFileName { get; set; }

    }
}
