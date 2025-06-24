using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.Security.Claims;

namespace BookShoptry.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly StoreContext _context;
        private readonly IMapper _mapper;

        public CartController(StoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCartForCustomer(int customerId)
        {
            var userId = int.Parse(User.FindFirstValue("id") ?? "0");
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Admin" && userId != customerId)
                return Unauthorized("You can only access your own cart.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                return NotFound("Cart not found.");

            var cartDto = _mapper.Map<CartDto>(cart);
            return Ok(cartDto);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue("id") ?? "0");
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Admin" && userId != dto.CustomerId)
                return Unauthorized("You can only modify your own cart.");

            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = dto.CustomerId,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
                return NotFound("Product not found");

            var cartItem = new CartItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                CartId = cart.Id,
                Product = product
            };

            cart.Items.Add(cartItem);
            await _context.SaveChangesAsync();

            var fullCart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var result = _mapper.Map<CartDto>(fullCart);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] CartItemCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue("id") ?? "0");
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Admin" && userId != dto.CustomerId)
                return Unauthorized("You can only modify your own cart.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null)
                return NotFound("Product not found in cart");

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            var result = new
            {
                customerId = cart.CustomerId,
                item = new
                {
                    productId = item.ProductId,
                    productTitle = item.Product?.Title,
                    price = item.Product?.Price,
                    quantity = item.Quantity
                }
            };

            return Ok(result);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] CartItemCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue("id") ?? "0");
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Admin" && userId != dto.CustomerId)
                return Unauthorized("You can only modify your own cart.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
                return NotFound("Cart not found");

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (cartItem == null)
                return NotFound("Product not found in cart");

            if (cartItem.Quantity <= dto.Quantity)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity -= dto.Quantity;
            }

            await _context.SaveChangesAsync();

            var updatedCart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var result = _mapper.Map<CartDto>(updatedCart);
            return Ok(result);
        }
    }
}
