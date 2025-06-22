using BookShoptry.Data;
using BookShoptry.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Dodanie StoreContext (EF Core)
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseInMemoryDatabase("StoreDb")); 

// Dodanie kontrolerów z konfiguracj¹ JSON - ignorowanie cykli
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddAutoMapper(typeof(Program));

// Dodanie Swaggera
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicjalizacja danych testowych
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StoreContext>();
    DbSeeder.Seed(context); 
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { } 
