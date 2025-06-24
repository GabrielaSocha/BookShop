using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.Security.Claims;
using System.Threading.Tasks;

public class ProductIntegrationTests
{
    private StoreContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("ProductIntegrationDb_" + System.Guid.NewGuid())
            .Options;
        return new StoreContext(options);
    }

    private IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>().ReverseMap();
            cfg.CreateMap<ProductCreateDto, Product>();
        });
        return config.CreateMapper();
    }

    private ProductsController GetController(StoreContext context, IMapper mapper, string role = "Admin")
    {
        var controller = new ProductsController(context, mapper);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task Post_ThenGet_ReturnsCorrectProduct()
    {
        // Arrange
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        var controller = GetController(context, mapper);

        var category = new Category { Id = 1, Name = "Test Category" };
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var newProduct = new ProductCreateDto
        {
            Title = "Integration Test Book",
            Description = "Integration Description",
            Author = "Integration Author",
            Price = 99.99m,
            CategoryId = category.Id
        };

        // Act – dodanie produktu
        var addResult = controller.Post(newProduct);
        var createdResult = Assert.IsType<CreatedAtActionResult>(addResult);
        var addedProduct = Assert.IsType<ProductDto>(createdResult.Value);

        // Act – pobranie produktu po ID
        var getResult = controller.Get(addedProduct.Id);
        var okGetResult = Assert.IsType<OkObjectResult>(getResult);
        var retrievedProduct = Assert.IsType<ProductDto>(okGetResult.Value);

        // Assert
        Assert.Equal(addedProduct.Id, retrievedProduct.Id);
        Assert.Equal("Integration Test Book", retrievedProduct.Title);
        Assert.Equal("Integration Description", retrievedProduct.Description);
        Assert.Equal("Integration Author", retrievedProduct.Author);
        Assert.Equal(99.99m, retrievedProduct.Price);
    }
}
