using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

public class CartIntegrationTests
{
    private StoreContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("CartIntegrationDb_" + System.Guid.NewGuid())
            .Options;
        return new StoreContext(options);
    }

    private IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductTitle, opt => opt.MapFrom(src => src.Product.Title));
            cfg.CreateMap<Cart, CartDto>();
        });
        return config.CreateMapper();
    }

    private CartController GetController(StoreContext context, IMapper mapper, string role, int userId)
    {
        var controller = new CartController(context, mapper);
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
    public async Task AddToCart_ThenGetCart_ReturnsCorrectItem()
    {
        // Arrange
        var context = GetInMemoryContext();
        var mapper = GetMapper();

        // Dodaj klienta
        var customer = new Customer { Id = 1, Username = "Test User" };
        await context.Customers.AddAsync(customer);

        // Dodaj produkt (z wymaganymi właściwościami)
        var product = new Product
        {
            Id = 1,
            Title = "Test Book",
            Price = 50,
            Author = "Test Author",
            Description = "Test Description"
        };
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Dodaj pusty koszyk klientowi
        var cart = new Cart { CustomerId = customer.Id, Items = new List<CartItem>() };
        await context.Carts.AddAsync(cart);
        await context.SaveChangesAsync();

        // Przygotuj kontroler z kontekstem użytkownika
        var controller = GetController(context, mapper, role: "User", userId: customer.Id);

        // Utwórz dane do dodania do koszyka
        var cartItemCreate = new CartItemCreateDto
        {
            CustomerId = customer.Id,
            ProductId = product.Id,
            Quantity = 2
        };

        // Act – dodaj do koszyka
        var addResult = await controller.AddToCart(cartItemCreate);
        var okResult = Assert.IsType<OkObjectResult>(addResult);
        var addedCart = Assert.IsType<CartDto>(okResult.Value);

        // Act – pobierz koszyk
        var getResult = await controller.GetCartForCustomer(customer.Id);
        var okCartResult = Assert.IsType<OkObjectResult>(getResult);
        var returnedCart = Assert.IsType<CartDto>(okCartResult.Value);


        // Assert
        Assert.Equal(customer.Id, returnedCart.CustomerId);
        Assert.Single(returnedCart.Items);
        Assert.Equal(product.Id, returnedCart.Items[0].ProductId);
        Assert.Equal(2, returnedCart.Items[0].Quantity);
        Assert.Equal(product.Title, returnedCart.Items[0].ProductTitle);
    }
}
