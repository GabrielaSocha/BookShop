using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Dtos;
using BookShoptry.Models;
using Microsoft.EntityFrameworkCore;

public class ProductsControllerTests
{
    private readonly Mock<StoreContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var context = new StoreContext(options);

        _mockContext = new Mock<StoreContext>(options);
        _mockMapper = new Mock<IMapper>();

        _controller = new ProductsController(context, _mockMapper.Object);
    }

    [Fact]
    public void Get_ReturnsAllProducts()
    {
        var contextOptions = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("TestDb_GetAll")
            .Options;

        using (var context = new StoreContext(contextOptions))
        {
            context.Products.AddRange(new List<Product>
        {
            new Product
            {
                Id = 1,
                Title = "Test Product 1",
                Author = "Author A",
                Description = "Description A",
                Price = 10.0m,
                Stock = 5,
                CategoryId = 1
            },
            new Product
            {
                Id = 2,
                Title = "Test Product 2",
                Author = "Author B",
                Description = "Description B",
                Price = 15.0m,
                Stock = 10,
                CategoryId = 1
            }
        });

            context.Categories.Add(new Category { Id = 1, Name = "Fiction" });
            context.SaveChanges();
        }

        using (var context = new StoreContext(contextOptions))
        {
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                      .Returns(new List<ProductDto> { new ProductDto(), new ProductDto() });

            var controller = new ProductsController(context, mockMapper.Object);

            var result = controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ProductDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
    }


    [Fact]
    public void Get_WithValidId_ReturnsProduct()
    {
        var contextOptions = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("TestDb_GetById")
            .Options;

        using (var context = new StoreContext(contextOptions))
        {
            context.Categories.Add(new Category { Id = 1, Name = "Fiction" });
            context.Products.Add(new Product
            {
                Id = 1,
                Title = "Test Book",
                Author = "Author A",
                Description = "Something",
                Price = 10,
                Stock = 10,
                CategoryId = 1
            });
            context.SaveChanges();
        }

        using (var context = new StoreContext(contextOptions))
        {
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                      .Returns(new ProductDto { Id = 1, Title = "Test Book" });

            var controller = new ProductsController(context, mockMapper.Object);

            var result = controller.Get(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal("Test Book", dto.Title);
        }
    }


    [Fact]
    public void Post_WithValidProduct_ReturnsCreatedProduct()
    {
        var contextOptions = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("TestDb_Post")
            .Options;

        var dto = new ProductCreateDto
        {
            Title = "New Book",
            Author = "Author Y",
            Description = "Some description",
            Price = 45.99m,
            CategoryId = 1
        };

        using (var context = new StoreContext(contextOptions))
        {
            context.Categories.Add(new Category { Id = 1, Name = "Fiction" });
            context.SaveChanges();
        }

        using (var context = new StoreContext(contextOptions))
        {
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<Product>(dto)).Returns(new Product
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId
            });

            mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                      .Returns(new ProductDto
                      {
                          Title = dto.Title,
                          Author = dto.Author,
                          Description = dto.Description,
                          Price = dto.Price,
                          CategoryName = "Fiction"
                      });

            var controller = new ProductsController(context, mockMapper.Object);

            var result = controller.Post(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var productDto = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(dto.Title, productDto.Title);
            Assert.Equal(dto.Price, productDto.Price);
        }
    }

}
