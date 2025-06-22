using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;

namespace BookShoptry.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly StoreContext _context;
        private readonly IMapper _mapper;

        public CartController(StoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET /Cart/user/1
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCartForCustomer(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product) // ← tutaj dociągamy dane produktu
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                return NotFound("Koszyk nie został znaleziony.");

            var cartDto = _mapper.Map<CartDto>(cart); // automatyczne mapowanie
            return Ok(cartDto);
        }
        // POST /Cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDto dto)
        {
            // Sprawdź, czy klient istnieje
            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            // Pobierz lub utwórz koszyk dla klienta
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
                await _context.SaveChangesAsync(); // zapisujemy, żeby mieć ID koszyka
            }

            // Pobierz produkt
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            // Dodaj przedmiot do koszyka
            var cartItem = new CartItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                CartId = cart.Id,
                Product = product // to zapewni dostęp do Title i Price
            };

            cart.Items.Add(cartItem);
            await _context.SaveChangesAsync();

            // Wczytaj pełny koszyk z produktami
            var fullCart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var result = _mapper.Map<CartDto>(fullCart);
            return Ok(result);
        }


        // PUT /Cart/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] CartItemCreateDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null)
            {
                return NotFound("Product not found in cart");
            }

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            // Mapuj tylko zmodyfikowaną pozycję
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



        // DELETE /Cart/remove/5
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] CartItemCreateDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (cartItem == null)
            {
                return NotFound("Product not found in cart");
            }

            if (cartItem.Quantity <= dto.Quantity)
            {
                // Usuwamy całą pozycję
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Zmniejszamy ilość
                cartItem.Quantity -= dto.Quantity;
            }

            await _context.SaveChangesAsync();

            // Zwróć zaktualizowany koszyk
            var updatedCart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            var result = _mapper.Map<CartDto>(updatedCart);
            return Ok(result);
        }

    }
}
