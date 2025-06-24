
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class CustomersControllerTests_New
{
    private CustomersController GetControllerWithUser(string role, int id, List<Customer> seedData)
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase(databaseName: "UnitTestDb_" + System.Guid.NewGuid())
            .Options;

        var context = new StoreContext(options);
        context.Customers.AddRange(seedData);
        context.SaveChanges();

        var controller = new CustomersController(context);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, role),
            new Claim("id", id.ToString())
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task GetAllCustomers_AdminRole_ReturnsAllCustomers()
    {
        var controller = GetControllerWithUser("Admin", 1, new List<Customer>
        {
            new Customer { Id = 1, Username = "A" },
            new Customer { Id = 2, Username = "B" }
        });

        var result = await controller.GetAllCustomers() as OkObjectResult;
        var customers = result.Value as List<Customer>;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(2, customers.Count);
    }

    [Fact]
    public async Task GetAllCustomers_NonAdminRole_ReturnsUnauthorized()
    {
        var controller = GetControllerWithUser("User", 2, new List<Customer>());

        var result = await controller.GetAllCustomers();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetCustomer_AdminRole_ReturnsCustomer()
    {
        var controller = GetControllerWithUser("Admin", 1, new List<Customer>
        {
            new Customer { Id = 5, Username = "Z" }
        });

        var result = await controller.GetCustomer(5) as OkObjectResult;
        var customer = result.Value as Customer;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(5, customer.Id);
    }

    [Fact]
    public async Task GetCustomer_NonAdminAccessingOthers_ReturnsUnauthorized()
    {
        var controller = GetControllerWithUser("User", 3, new List<Customer>
        {
            new Customer { Id = 4, Username = "Y" }
        });

        var result = await controller.GetCustomer(4);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetCustomer_OwnProfile_ReturnsCustomer()
    {
        var controller = GetControllerWithUser("User", 6, new List<Customer>
        {
            new Customer { Id = 6, Username = "X" }
        });

        var result = await controller.GetCustomer(6) as OkObjectResult;
        var customer = result.Value as Customer;

        Assert.NotNull(result);
        Assert.Equal(6, customer.Id);
    }

    [Fact]
    public async Task GetCustomer_NotFound_ReturnsNotFound()
    {
        var controller = GetControllerWithUser("Admin", 1, new List<Customer>());

        var result = await controller.GetCustomer(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
