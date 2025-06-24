using BookShoptry.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly StoreContext _context;
    private readonly IEmailService _emailService;

    public OrdersController(StoreContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [Authorize]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized("Invalid token");

        var user = await _context.Customers.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            return NotFound("User not found");

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == user.Id);

        if (cart == null || cart.Items.Count == 0)
            return BadRequest("Cart is empty");

        // ✉️ Stwórz treść „paragonu”
        var sb = new StringBuilder();
        sb.AppendLine("Your order summary:");
        sb.AppendLine("-----------------------");

        decimal total = 0;
        foreach (var item in cart.Items)
        {
            var line = $"{item.Product.Title} - {item.Quantity} x {item.Product.Price} = {item.Quantity * item.Product.Price}";
            sb.AppendLine(line);
            total += item.Quantity * item.Product.Price;
        }

        sb.AppendLine("-----------------------");
        sb.AppendLine($"Total: {total} PLN");

        // ✉️ Wyślij
        await _emailService.SendEmailAsync(user.Email, "Your Receipt from BookShop", sb.ToString());

        // (opcjonalnie) Czyść koszyk po wysyłce
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();

        return Ok("Order confirmed and receipt sent.");
    }
}
