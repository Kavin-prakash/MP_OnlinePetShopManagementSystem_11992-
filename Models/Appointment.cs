using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace OnlinePetShopManagementSystem.Models
{
    public class Appointment
    {

        [Key]
        public int AppointmentId { get; set; }

        public string Date { get; set; }


        public string? Time { get; set; }

        public PetDetail PetDetails  {get;set;}

      

        public User Users { get; set; }




        //public int UserId { get; set; }











    }
}
