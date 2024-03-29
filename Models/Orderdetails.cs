using System.ComponentModel.DataAnnotations;

namespace OnlinePetShopManagementSystem.Models
{
    public class Orderdetails
    {
        [Key]
        public int OrderID { get; set; }


       public User Users { get; set; }


       public PetAccessory Accessory { get; set; }
     }
}
