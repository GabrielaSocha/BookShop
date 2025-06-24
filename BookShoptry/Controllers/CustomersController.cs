using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookShoptry.Data;
using BookShoptry.Models;
using System.Security.Claims;

namespace BookShoptry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly StoreContext _context;

    public CustomersController(StoreContext context)
    {
        _context = context;
    }

    // GET: api/Customers (tylko dla Admina)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllCustomers()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
        {
            return Unauthorized("You can only access your own data.");
        }
        var customers = await _context.Customers.ToListAsync();
        return Ok(customers);
    }

    // GET: api/Customers/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = int.Parse(User.FindFirst("id")!.Value);

        // Jeśli nie admin, to może pobrać tylko własny profil
        if (userRole != "Admin" && userId != id)
        {
            return Unauthorized("You can only access your own data.");
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound("Customer not found");
        }

        return Ok(customer);
    }
}
