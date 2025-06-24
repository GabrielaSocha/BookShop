
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

public class CartControllerTests_New
{
    private StoreContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("CartTestDb_" + System.Guid.NewGuid())
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
    public async Task GetCart_AdminAccess_ReturnsCart()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        context.Carts.Add(new Cart { Id = 1, CustomerId = 2 });
        context.SaveChanges();

        var controller = GetController(context, mapper, "Admin", 1);
        var result = await controller.GetCartForCustomer(2) as OkObjectResult;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCart_OwnerAccess_ReturnsCart()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        context.Carts.Add(new Cart { Id = 2, CustomerId = 3 });
        context.SaveChanges();

        var controller = GetController(context, mapper, "User", 3);
        var result = await controller.GetCartForCustomer(3) as OkObjectResult;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetCart_OtherUser_ReturnsUnauthorized()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        context.Carts.Add(new Cart { Id = 3, CustomerId = 4 });
        context.SaveChanges();

        var controller = GetController(context, mapper, "User", 99);
        var result = await controller.GetCartForCustomer(4);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetCart_NotFound_ReturnsNotFound()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        var controller = GetController(context, mapper, "Admin", 1);

        var result = await controller.GetCartForCustomer(100);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
