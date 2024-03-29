using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
using OnlinePetShopManagementSystem.Models;
using Org.BouncyCastle.Asn1.Cmp;
using System.Net.Mail;
using System.Net;


namespace OnlinePetShopManagementSystem.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OnlinePetShopController : Controller
    {
        private readonly PetShopDbContext _dbcontext;

        private readonly IWebHostEnvironment _environment;

        private readonly IConfiguration _configuration;

        private readonly IEmailService _emailService;


        public OnlinePetShopController(PetShopDbContext dbcontext, IWebHostEnvironment environment, IConfiguration configuration, IEmailService emailService)
        {
            _dbcontext = dbcontext;

            _environment = environment;

            _configuration = configuration;

            _emailService = emailService;

        }

       

        [HttpPost]
        public async Task<ActionResult> PostUser([FromBody] UserDtoApp userDto)
        {
            // Create a new User object and map properties from UserDtoApp
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                Password = userDto.Password
            };

            // Add the User object to the database context
            _dbcontext.Users.Add(user);
            await _dbcontext.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]

        public async Task<ActionResult> Checkuser(LoginUser login)
        {
            if (_dbcontext.Users.Any(s => s.Email == login.Email))
            {
                var checkuser = _dbcontext.Users.Where(s => s.Email == login.Email).FirstOrDefault();

                if (checkuser.Password == login.Password)
                {
                    return Ok(checkuser.Id);
                }
            }
            return NotFound();

        }
       
        [HttpGet]
        public IActionResult Get()
        {
            var appointment = _dbcontext.Appointments.Include(x => x.Users).Include(a=>a.PetDetails).ToList();
            return Ok(appointment);

        }



        // Post Admin Email and Password

        [HttpPost]
        public async Task<ActionResult> PostAdmin(LoginAdmin admin)

        {
            _dbcontext.Admin.Add(admin);
            await _dbcontext.SaveChangesAsync();
            return Ok();


        }


        [HttpPost]

        public async Task<ActionResult> CheckAdmin(Checkadmin checkadmin)
        {
            if (_dbcontext.Admin.Any(s => s.Email == checkadmin.Email))
            {
                var checkuser = _dbcontext.Admin.Where(s => s.Email == checkadmin.Email).FirstOrDefault();

                if (checkuser.Password == checkadmin.Password)
                {
                    return Ok();
                }
            }
            return NotFound();

        }






        // ----------------------Methods for creating Pet details------------------////////

        [HttpPost]
        public async Task<IActionResult> CreatePet([FromForm] PetDtoApp petDto)
        {
            // Generate a unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{petDto.PetImage.FileName}";

            // Save the image to a designated folder (e.g., wwwroot/images)
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await petDto.PetImage.CopyToAsync(stream);
            }


            // Create a new PetDetail object and map properties from PetDtoApp
            var petDetail = new PetDetail
            {
                PetName = petDto.PetName,
                PetCategory = petDto.PetCategory,
                PetBreed = petDto.PetBreed,
                PetPrice = petDto.PetPrice,
                PetDescription = petDto.PetDescription,
                PetStock = petDto.PetStock,
                UniqueFileName = uniqueFileName // Store the unique file name
            };

            // Add the PetDetail object to the database context
            _dbcontext.PetDetails.Add(petDetail);
            await _dbcontext.SaveChangesAsync();

            // Return the image URL or any other relevant response
            // Assuming you want to return the path of the saved image
            var imageUrl = $"{Request.Scheme}://{Request.Host}/Images/{uniqueFileName}";
            return Ok(new { ImageUrl = imageUrl });
        }





        // delete method for the petdetail


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePetDetails(int id)
        {
            var petdetails = _dbcontext.PetDetails.Find(id);
            if (petdetails == null)
            {
                return NotFound(); // PetAccessory not found
            }

            _dbcontext.PetDetails.Remove(petdetails);
            await _dbcontext.SaveChangesAsync();

            return NoContent(); // Successfully deleted
        }


        // Update Method for the Petdetails //



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, [FromForm] PetDetail pet)
        {
            // Retrieve the existing pet from the database
            var petToUpdate = await _dbcontext.PetDetails.FindAsync(id);
            if (petToUpdate == null)
            {
                return NotFound();
            }

            // Check if a new image is provided
            if (pet.PetImage != null)
            {
                // Generate a unique file name for the new image
                var uniqueFileName = $"{Guid.NewGuid()}_{pet.PetImage.FileName}";

                // Save the new image to the designated folder
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await pet.PetImage.CopyToAsync(stream);
                }

                // Delete the old image if necessary
                var oldFilePath = Path.Combine(uploadsFolder, petToUpdate.UniqueFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the file path in the database
                petToUpdate.UniqueFileName = uniqueFileName;
            }

            // Update individual properties if provided
            if (!string.IsNullOrEmpty(pet.PetName))
            {
                petToUpdate.PetName = pet.PetName;
            }

            if (!string.IsNullOrEmpty(pet.PetCategory))
            {
                petToUpdate.PetCategory = pet.PetCategory;
            }

            if (!string.IsNullOrEmpty(pet.PetBreed))
            {
                petToUpdate.PetBreed = pet.PetBreed;
            }

            if (pet.PetPrice > 0)
            {
                petToUpdate.PetPrice = pet.PetPrice;
            }

            if (pet.PetStock > 0)
            {
                petToUpdate.PetStock = pet.PetStock;
            }

            if (!string.IsNullOrEmpty(pet.PetDescription))
            {
                petToUpdate.PetDescription = pet.PetDescription;
            }

            // Save the changes to the database
            _dbcontext.SaveChanges();

            // Return a success response, possibly with the updated pet data
            return Ok(petToUpdate);
        }







        //---Methods for get all the image details form the folder -------//

        [HttpGet]
        public IActionResult GetAllPet()
        {
            var pets = _dbcontext.PetDetails.ToList();

            var petList = new List<object>();

            foreach (var pet in pets)
            {

                // Create an object containing cart details and image URL
                var petData = new
                {
                    id = pet.Id,
                    petname = pet.PetName,
                    category = pet.PetCategory,
                    price = pet.PetPrice,
                    breed=pet.PetBreed,
                    description = pet.PetDescription,
                    stock=pet.PetStock,
                    imageUrl = String.Format("{0}://{1}{2}/wwwroot/images/{3}", Request.Scheme, Request.Host, Request.PathBase, pet.UniqueFileName)
                };

                petList.Add(petData);
            }

            return Ok(petList);
        }


        [HttpGet("{id}")]
        public IActionResult GetPetById(int id)
        {
            var pet = _dbcontext.PetDetails.FirstOrDefault(a => a.Id == id);

            if (pet == null)
            {
                return NotFound();
            }

            var petAccessoryData = new
            {
                id = pet.Id,
                petName = pet.PetName,
                petCategory = pet.PetCategory,
                petPrice = pet.PetPrice,
                petBreed = pet.PetBreed,
                petStock = pet.PetStock,
                petDescription = pet.PetDescription,
                imageUrl = String.Format("{0}://{1}{2}/wwwroot/images/{3}", Request.Scheme, Request.Host, Request.PathBase, pet.UniqueFileName)
            };

            return Ok(petAccessoryData);
        }




        //-----Controller code Get a image by an id ---------------////

        [HttpGet("{id}/Image")]
        public IActionResult GetImage(int id)
        {
            var pet = _dbcontext.PetDetails.Find(id);
            if (pet == null)
            {
                return NotFound(); // User not found
            }

            // Construct the full path to the image file

            var imagePath = Path.Combine(_environment.WebRootPath, "Images", pet.UniqueFileName);

            // Check if the image file exists
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound(); // Image file not found
            }

            // Serve the image file
            return PhysicalFile(imagePath, "Image/jpeg");
        }



        ////////--------------------PetAccessoryDetails------------////////////////////


        //Post petaccessory products


        [HttpPost]
        public async Task<IActionResult> CreatePetAccessory([FromForm] PetAccessory petaccessory)
        {

            // Generate a unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{petaccessory.AccessoryImage.FileName}";

            // Save the image to a designated folder (e.g., wwwroot/images)
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await petaccessory.AccessoryImage.CopyToAsync(stream);
            }

            // Store the file path in the database
            petaccessory.UniqueFileName = uniqueFileName;


            _dbcontext.Accessory.Add(petaccessory);
            await _dbcontext.SaveChangesAsync();
            // var imageUrl = $"{Request.Scheme}://{Request.Host}/images/{cart.UniqueFileName}";

            // Return the image URL or any other relevant response
            return Ok();

        }





        //Get all PetAccessory details

        [HttpGet]
        public IActionResult GetAllPetAccessoryy()
        {
            var petaccessories = _dbcontext.Accessory.ToList();

            var petAccessoryList = new List<object>();

            foreach (var pet in petaccessories)
            {

                // Create an object containing cart details and image URL
                var petData = new
                {
                    id = pet.Id,
                    petaccessoryname = pet.PetAccesoryName,
                    petaccessoryprice = pet.PetAccessoryPrice,
                    petaccessoryimage = pet.AccessoryImage,
                    petaccessorydescription = pet.PetAccessoryDescription,
                    imageUrl = String.Format("{0}://{1}{2}/wwwroot/images/{3}", Request.Scheme, Request.Host, Request.PathBase, pet.UniqueFileName)
                };

                petAccessoryList.Add(petData);
            }

            return Ok(petAccessoryList);
        }





        [HttpGet("{id}")]
        public IActionResult GetPetAccessoryById(int id)
        {
            var petAccessory = _dbcontext.Accessory.FirstOrDefault(a => a.Id == id);

            if (petAccessory == null)
            {
                return NotFound();
            }

            var petAccessoryData = new
            {
                id = petAccessory.Id,
                petaccessoryname = petAccessory.PetAccesoryName,
                petaccessoryprice = petAccessory.PetAccessoryPrice,
                petaccessoryimage = petAccessory.AccessoryImage,
                petaccessorydescription = petAccessory.PetAccessoryDescription,
                imageUrl = String.Format("{0}://{1}{2}/wwwroot/images/{3}", Request.Scheme, Request.Host, Request.PathBase, petAccessory.UniqueFileName)
            };

            return Ok(petAccessoryData);
        }



        //  Delete Method Delete the petaccessort details

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePetAccessory(int id)
        {
            var petAccessory = _dbcontext.Accessory.Find(id);
            if (petAccessory == null)
            {
                return NotFound(); // PetAccessory not found
            }

            _dbcontext.Accessory.Remove(petAccessory);
            await _dbcontext.SaveChangesAsync();


            return NoContent(); // Successfully deleted
        }



        // Pet Accessory Product get by a Id 




        [HttpGet("{id}/Image")]
        public IActionResult GetAccessoryImage(int id)
        {
            var petaccessory= _dbcontext.Accessory.Find(id);
            if (petaccessory == null)
            {
                return NotFound(); // User not found
            }

            // Construct the full path to the image file

            var imagePath = Path.Combine(_environment.WebRootPath, "Images", petaccessory.UniqueFileName);

            // Check if the image file exists
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound(); // Image file not found
            }

            // Serve the image file
            return PhysicalFile(imagePath, "Image/jpeg");
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePetAccessory(int id, [FromForm] PetAccessory pet)
        {
            // Retrieve the existing pet from the database
            var petToUpdate = await _dbcontext.Accessory.FindAsync(id);
            if (petToUpdate == null)
            {
                return NotFound();
            }

            // Check if a new image is provided
            if (pet.AccessoryImage != null)
            {
                // Generate a unique file name for the new image
                var uniqueFileName = $"{Guid.NewGuid()}_{pet.AccessoryImage.FileName}";

                // Save the new image to the designated folder
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await pet.AccessoryImage.CopyToAsync(stream);
                }

                // Delete the old image if necessary
                var oldFilePath = Path.Combine(uploadsFolder, petToUpdate.UniqueFileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the file path in the database
                petToUpdate.UniqueFileName = uniqueFileName;
            }
            
            // Update individual properties if provided
            if (!string.IsNullOrEmpty(pet.PetAccesoryName))
            {
                petToUpdate.PetAccesoryName = pet.PetAccesoryName;
            }

            if (!string.IsNullOrEmpty(pet.PetAccessoryPrice))
            {
                petToUpdate.PetAccessoryPrice = pet.PetAccessoryPrice;
            }

            if (!string.IsNullOrEmpty(pet.PetAccessoryDescription))
            {
                petToUpdate.PetAccessoryDescription = pet.PetAccessoryDescription;
            }


            // Save the changes to the database
            _dbcontext.SaveChanges();

            // Return a success response, possibly with the updated pet data
            return Ok(petToUpdate);
        }





        //[HttpPost]

        //public async Task<ActionResult> PostAppointment(DumModelAppointment appointment)

        //{
        //    User appointmentuser = _dbcontext.Users.Find(appointment.UserID);

        //    PetDetail appointmentpetdetails = _dbcontext.PetDetails.Find(appointment.PetDetailsID);


        //    Appointment appointmentdata = new Appointment() {

        //      Date= appointment.GetDate,
        //      Time= appointment.GetTime,
        //        PetDetails = appointmentpetdetails,
        //        Users = appointmentuser

        //    };

        //     _dbcontext.Appointments.Add(appointmentdata);

        //    await _dbcontext.SaveChangesAsync();

        //    return Ok();

        //}

        //
        [HttpPost]

        public async Task<ActionResult> PostAppointment([FromBody] DumModelAppointment appointmentDto)

        {
            User appointmentuser = _dbcontext.Users.FirstOrDefault(s => s.Id == Convert.ToInt16(appointmentDto.userid));

            PetDetail pet = _dbcontext.PetDetails.FirstOrDefault(s => s.Id == Convert.ToInt16(appointmentDto.petid));



            Appointment appointment = new Appointment()
            {
                AppointmentId = 0,
                Date = appointmentDto.GetDate,
                Time = appointmentDto.GetTime,
                PetDetails = pet,
                Users = appointmentuser
            };

            _dbcontext.Appointments.Add(appointment);

            await _dbcontext.SaveChangesAsync();
            await SendEmailToAdminAsync(appointmentuser,pet);

            return Ok();

        }



        // [HttpPost]
        //[Route("api/OnlinePetShop/PostAppointment/{userId}/{petId}")]
        //public async Task<ActionResult> PostAppointment([FromBody] DumModelAppointment appointmentDto, int userId, int petId)
        //{
        //    try
        //    {
        //        // Fetch user and pet details
        //        User appointmentUser = _dbcontext.Users.FirstOrDefault(s => s.Id == userId);
        //        PetDetail pet = _dbcontext.PetDetails.FirstOrDefault(s => s.Id == petId);

        //        if (appointmentUser == null || pet == null)
        //        {
        //            return NotFound("User or pet not found");
        //        }

        //        // Create appointment with date, time, and related user and pet
        //        Appointment appointment = new Appointment()
        //        {
        //            AppointmentId = 0,
        //            Date = appointmentDto.GetDate,
        //            Time = appointmentDto.GetTime,
        //            PetDetails = pet,
        //            Users = appointmentUser
        //        };

        //        // Add and save appointment
        //        _dbcontext.Appointments.Add(appointment);
        //        await _dbcontext.SaveChangesAsync();

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message); // Return appropriate error response
        //    }
        //}




        [HttpGet("PetBooked/{userid}")]
        public async Task<IActionResult> GetAppointmnetDetails(int userid)
        {
            var appointment = await _dbcontext.Appointments
                .Where(ld => ld.Users.Id == userid)
                .Include(ld => ld.Users)
                .Include(ld => ld.PetDetails) // Include related LeaveTypes
                
                .ToListAsync();


            // Map to a simplified DTO (Data Transfer Object) if needed
            var appDto = appointment.Select(ld => new
            {
                ld.Date,
                ld.Time,
                UserName = ld.Users?.FirstName,
                UserPhone=ld.Users?.PhoneNumber,
                petname=ld.PetDetails?.PetName

            });

            return Ok(appDto);
        }


        [HttpPost]
        public IActionResult CreateOrder([FromBody] OrderDto orderDto)
        {
            // Assuming you have a User and PetAccessory DbSet in your DbContext
            var orderUser = _dbcontext.Users.FirstOrDefault(s => s.Id == Convert.ToInt16(orderDto.UserId));
            var product = _dbcontext.Accessory.FirstOrDefault(s => s.Id == Convert.ToInt16(orderDto.ProductId));

            if (orderUser == null || product == null)
            {
                return BadRequest("Invalid user or product ID.");
            }

            var order = new Orderdetails
            {
                OrderID=0,
                Users = orderUser,
                Accessory = product
            };

            _dbcontext.Orderdetails.Add(order);
            _dbcontext.SaveChanges();

            return Ok("Order created successfully.");
        }


        //[HttpGet]
        //public IActionResult GetcartItems()
        //{
        //    var appointment = _dbcontext.Orderdetails.Include(x => x.Users).Include(a => a.Accessory).ToList();
        //    return Ok(appointment);

        //}

        [HttpGet]
        public IActionResult GetcartItems()
        {
            var cartItems = _dbcontext.Orderdetails
                .Include(x => x.Users)
                .Include(a => a.Accessory)
                .Select(c => new
                {
                  // Include other necessary fields from the Orderdetails, Users, and Accessory

                    id = c.OrderID,
                    accessoryId = c.Accessory.Id,
                    accessoryName = c.Accessory.PetAccesoryName,
                    accessoryPrice = c.Accessory.PetAccessoryPrice,
                    accessoryDescription = c.Accessory.PetAccessoryDescription,
                    imageUrl = String.Format("{0}://{1}{2}/wwwroot/images/{3}", Request.Scheme, Request.Host, Request.PathBase, c.Accessory.UniqueFileName)
                })
                .ToList();

            return Ok(cartItems);
        }





        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserCartItems(int id)
        {
            var cartItems = await _dbcontext.Orderdetails
                .Where(x => x.Users.Id == id)
                .Include(x => x.Users)
                .Include(a => a.Accessory)
                .ToListAsync(); // Use the converted integer for comparison

            var cart = cartItems.Select(c => new
            {
                // Include other necessary fields from the Orderdetails, Users, and Accessory
                id = c.OrderID,
                accessoryId = c.Accessory.Id,
                accessoryName = c.Accessory.PetAccesoryName,
                accessoryPrice = c.Accessory.PetAccessoryPrice,
                accessoryDescription = c.Accessory.PetAccessoryDescription,
                imageUrl = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, c.Accessory.UniqueFileName)
            });

            return Ok(cart);
        }








        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = _dbcontext.Orderdetails.Find(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            _dbcontext.Orderdetails.Remove(order);
            await _dbcontext.SaveChangesAsync();

            return Ok($"Order with ID {id} deleted successfully.");
        }

        private async Task SendEmailToAdminAsync(User users,PetDetail petDetail)
        {
            try
            {
                
                var email = users.Email;
                var name = users.FirstName;
                var petname = petDetail.PetName;
                
                var adminEmail = email; // Replace with actual admin email address
                var subject = $"Your Booking is successful";
                var body = $"Hi {name},\n Thank You for choosing our Pet shop!.Pet Name :{petname} is booked  successfully. ";

                await _emailService.SendEmailAsync(adminEmail, subject, body);
                // Log success or handle any exceptions
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return;
            }
        }

    }
}
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}



// Implement your email service (e.g., using SmtpClient)
public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {

        using (var client = new SmtpClient("smtp-mail.outlook.com"))
        {
            client.Port = 587;
            client.Credentials = new NetworkCredential("ashamold2002@gmail.com", "AshaShneha_._2705");
            client.EnableSsl = true;

            var message = new MailMessage
            {
                From = new MailAddress("ashamold2002@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}

