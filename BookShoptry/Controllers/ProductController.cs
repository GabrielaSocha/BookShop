using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BookShoptry.Data;
using BookShoptry.Dtos;
using BookShoptry.Models;
using AutoMapper;
using System.Security.Claims;

namespace BookShoptry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly StoreContext _context;
        private readonly IMapper _mapper;

        public ProductsController(StoreContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            if (!products.Any())
                return Ok("No products available.");

            var dtoList = _mapper.Map<List<ProductDto>>(products);
            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound($"Product with ID {id} was not found.");

            var dto = _mapper.Map<ProductDto>(product);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] ProductCreateDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
                return Unauthorized("Access denied: only administrators can add products.");

            if (!ModelState.IsValid)
                return BadRequest("Invalid product data.");

            var category = _context.Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
            if (category == null)
                return BadRequest($"Category with ID {dto.CategoryId} does not exist.");

            var product = _mapper.Map<Product>(dto);
            product.CategoryId = dto.CategoryId;

            _context.Products.Add(product);
            _context.SaveChanges();

            var result = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
                return Unauthorized("Access denied: only administrators can update products.");

            if (!ModelState.IsValid)
                return BadRequest("Invalid product data.");

            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound($"Product with ID {id} was not found.");

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
            if (category == null)
                return BadRequest($"Category with ID {dto.CategoryId} does not exist.");

            product.Title = dto.Title;
            product.Author = dto.Author;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Category = category;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product updated successfully.",
                productId = product.Id,
                product.Title,
                product.Author,
                product.Description,
                product.Price,
                categoryName = product.Category.Name
            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
                return Unauthorized("Access denied: only administrators can delete products.");

            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound($"Product with ID {id} was not found.");

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok($"Product with ID {id} was successfully deleted.");
        }
    }
}
