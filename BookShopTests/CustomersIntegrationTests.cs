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

namespace BookShopTests;

public class CustomersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly StoreContext _db;

    public CustomersIntegrationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("TestDb_Customers");
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
        _db.Customers.RemoveRange(_db.Customers);
        await _db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCustomers_ReturnsEmptyListInitially()
    {
        var response = await _client.GetAsync("/api/customers");
        response.EnsureSuccessStatusCode();

        var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
        Assert.NotNull(customers);
        Assert.Empty(customers);
    }
}
