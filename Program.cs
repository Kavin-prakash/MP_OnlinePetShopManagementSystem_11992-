using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OnlinePetShopManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEmailService, EmailService>();



var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PetShopDbContext>(options => options.UseMySql(ConnectionString, new MySqlServerVersion(new Version())));


builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseCors(options => options.WithOrigins("http://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, "Images")),
    RequestPath = "/wwwroot/Images"
});



app.UseAuthorization();

app.MapControllers();

app.UseCors("myAppCors");

app.Run();
