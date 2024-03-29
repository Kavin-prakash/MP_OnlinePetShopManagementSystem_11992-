//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace OnlinePetShopManagementSystem.Models
{
    public class PetShopDbContext:DbContext
    {
        public PetShopDbContext(DbContextOptions<PetShopDbContext> options) : base(options)
        {


        }

        public DbSet<User> Users{ get; set; }

        public DbSet<PetAccessory> Accessory { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<PetDetail> PetDetails { get; set; }

        public DbSet<LoginAdmin> Admin{ get; set; }

        public DbSet<Orderdetails> Orderdetails { get; set; }
        
    }
}
