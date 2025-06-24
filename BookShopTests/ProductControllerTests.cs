
using Xunit;
using Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookShoptry.Controllers;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.Collections.Generic;
using System.Linq;

public class ProductsControllerTests_New
{
    private StoreContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
            .Options;

        var context = new StoreContext(options);
        return context;
    }

    private IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        });

        return config.CreateMapper();
    }

    [Fact]
    public void Get_ReturnsProductList()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();

        var category = new Category { Id = 1, Name = "Science" };
        context.Categories.Add(category);
        context.Products.Add(new Product
        {
            Id = 1,
            Title = "Book",
            Author = "Author",
            Description = "Desc",
            Price = 10,
            Stock = 5,
            Category = category
        });
        context.SaveChanges();

        var controller = new ProductsController(context, mapper);
        var result = controller.Get() as OkObjectResult;

        Assert.NotNull(result);
        var dtoList = result.Value as List<ProductDto>;
        Assert.Single(dtoList);
        Assert.Equal("Science", dtoList[0].CategoryName);
    }

    [Fact]
    public void Get_ReturnsNoProductsMessage()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        var controller = new ProductsController(context, mapper);

        var result = controller.Get() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal("No products available.", result.Value);
    }

    [Fact]
    public void GetById_ExistingProduct_ReturnsProduct()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();

        var category = new Category { Id = 1, Name = "History" };
        var product = new Product
        {
            Id = 2,
            Title = "History Book",
            Author = "HistAuthor",
            Description = "Desc",
            Price = 20,
            Stock = 3,
            Category = category
        };
        context.Categories.Add(category);
        context.Products.Add(product);
        context.SaveChanges();

        var controller = new ProductsController(context, mapper);
        var result = controller.Get(2) as OkObjectResult;

        Assert.NotNull(result);
        var dto = result.Value as ProductDto;
        Assert.Equal("History Book", dto.Title);
        Assert.Equal("History", dto.CategoryName);
    }

    [Fact]
    public void GetById_NonExistingProduct_ReturnsNotFound()
    {
        var context = GetInMemoryContext();
        var mapper = GetMapper();
        var controller = new ProductsController(context, mapper);

        var result = controller.Get(99);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
