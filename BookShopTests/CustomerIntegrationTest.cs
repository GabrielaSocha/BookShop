using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomerIntegrationTests
{
    private StoreContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("CustomerIntegrationDb_" + System.Guid.NewGuid())
            .Options;
        return new StoreContext(options);
    }

    private CustomersController GetCustomerController(StoreContext context, int userId, string role = "User")
    {
        var controller = new CustomersController(context);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("id", userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task Register_ThenGetById_ReturnsOwnCustomer()
    {
        // Arrange
        var context = GetInMemoryContext();

        var customer = new Customer
        {
            Id = 1,
            Username = "Test User",
            Email = "test@example.com",
            PasswordHash = "Secret123"
        };

        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var controller = GetCustomerController(context, userId: 1, role: "User");

        // Act
        var result = await controller.GetCustomer(1);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCustomer = Assert.IsType<Customer>(okResult.Value);

        // Assert
        Assert.Equal(customer.Id, returnedCustomer.Id);
        Assert.Equal("Test User", returnedCustomer.Username);
        Assert.Equal("test@example.com", returnedCustomer.Email);
    }
}
