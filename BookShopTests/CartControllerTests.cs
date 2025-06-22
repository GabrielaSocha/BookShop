using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CartControllerTests
{
    [Fact]
    public async Task GetCartForCustomer_ReturnsCart()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("GetCart_Customer")
            .Options;

        using (var context = new StoreContext(options))
        {
            context.Customers.Add(new Customer { Id = 1, Username = "Jan" });
            context.Products.Add(new Product { Id = 1, Title = "Book", Author = "A", Description = "D", Price = 10, Stock = 5, CategoryId = 1 });
            context.Carts.Add(new Cart
            {
                Id = 1,
                CustomerId = 1,
                Items = new List<CartItem> {
                    new CartItem { ProductId = 1, Quantity = 2 }
                }
            });
            context.SaveChanges();
        }

        using (var context = new StoreContext(options))
        {
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<CartDto>(It.IsAny<Cart>())).Returns(new CartDto { CustomerId = 1 });

            var controller = new CartController(context, mockMapper.Object);
            var result = await controller.GetCartForCustomer(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<CartDto>(okResult.Value);
            Assert.Equal(1, dto.CustomerId);
        }
    }

    [Fact]
    public async Task AddToCart_CreatesCartIfNotExists()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("AddToCart_Creates")
            .Options;

        var dto = new CartItemCreateDto
        {
            CustomerId = 2,
            ProductId = 1,
            Quantity = 3
        };

        using (var context = new StoreContext(options))
        {
            context.Customers.Add(new Customer { Id = 2, Username = "Anna" });
            context.Products.Add(new Product
            {
                Id = 1,
                Title = "Book",
                Author = "A",
                Description = "D",
                Price = 15,
                Stock = 10,
                CategoryId = 1
            });
            context.Categories.Add(new Category { Id = 1, Name = "Books" });
            context.SaveChanges();
        }

        using (var context = new StoreContext(options))
        {
            var mockMapper = new Mock<IMapper>();

            mockMapper.Setup(m => m.Map<CartItem>(dto)).Returns(new CartItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });

            var controller = new CartController(context, mockMapper.Object);

            // Act
            var result = await controller.AddToCart(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var cart = await context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == 2);
            Assert.NotNull(cart);

            var item = cart.Items.FirstOrDefault();
            Assert.NotNull(item);
            Assert.Equal(1, item.ProductId);
            Assert.Equal(3, item.Quantity);
        }
    }

}
