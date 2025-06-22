using BookShoptry.Data;
using BookShoptry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShoptry.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly StoreContext _context;

    public CustomersController(StoreContext context) => _context = context;
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
    {
        var customers = await _context.Customers.ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public IActionResult GetCustomer(int id)
    {
        var customer = _context.Customers.Find(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }
}

