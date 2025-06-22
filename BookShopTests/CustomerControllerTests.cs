using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;

public class CustomersControllerTests
{
    [Fact]
    public async Task GetAllCustomers_ReturnsAll()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("GetAllCustomers")
            .Options;

        using (var context = new StoreContext(options))
        {
            context.Customers.AddRange(
                new Customer { Id = 1, Username = "alice", Email = "a@x.com", PasswordHash = "hash1" },
                new Customer { Id = 2, Username = "bob", Email = "b@x.com", PasswordHash = "hash2" }
            );
            context.SaveChanges();
        }

        using (var context = new StoreContext(options))
        {
            var controller = new CustomersController(context);
            var result = await controller.GetAllCustomers();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var customers = Assert.IsType<List<Customer>>(okResult.Value);
            Assert.Equal(2, customers.Count);
            Assert.Contains(customers, c => c.Username == "alice");
            Assert.Contains(customers, c => c.Username == "bob");
        }
    }

    [Fact]
    public void GetCustomer_WithValidId_ReturnsCustomer()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("GetCustomerById")
            .Options;

        using (var context = new StoreContext(options))
        {
            context.Customers.Add(new Customer
            {
                Id = 10,
                Username = "eve",
                Email = "eve@example.com",
                PasswordHash = "securehash"
            });
            context.SaveChanges();
        }

        using (var context = new StoreContext(options))
        {
            var controller = new CustomersController(context);
            var result = controller.GetCustomer(10);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var customer = Assert.IsType<Customer>(okResult.Value);
            Assert.Equal("eve", customer.Username);
        }
    }
}
