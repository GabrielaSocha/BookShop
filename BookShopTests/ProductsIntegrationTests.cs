using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookShoptry;
using BookShoptry.Data;
using BookShoptry.Dtos;
using BookShoptry.Models;
using System.Linq;
using System.Collections.Generic;

public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly StoreContext _db;

    public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var scopedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StoreContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<StoreContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<StoreContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        });

        _client = scopedFactory.CreateClient();

        var sp = scopedFactory.Services.CreateScope().ServiceProvider;
        _db = sp.GetRequiredService<StoreContext>();
    }

    public async Task InitializeAsync()
    {
        _db.Products.RemoveRange(_db.Products);
        _db.Categories.RemoveRange(_db.Categories);
        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProducts_ReturnsEmptyListInitially()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task PostAndGetProduct_Works()
    {
        _db.Categories.Add(new Category { Id = 1, Name = "Fiction" });
        _db.SaveChanges();

        var dto = new ProductCreateDto
        {
            Title = "Test Book",
            Author = "Author A",
            Description = "Description here",
            Price = 45,
            CategoryId = 1
        };

        var postResponse = await _client.PostAsJsonAsync("/api/products", dto);
        postResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync("/api/products");
        getResponse.EnsureSuccessStatusCode();

        var products = await getResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.Single(products);
        Assert.Equal("Test Book", products[0].Title);
    }
}