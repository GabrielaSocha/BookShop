using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookShoptry.Data;
using BookShoptry.Dtos;
using BookShoptry.Models;
using AutoMapper;

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

        // GET: api/Products
        [HttpGet]
        public IActionResult Get()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            var dtoList = _mapper.Map<List<ProductDto>>(products);
            return Ok(dtoList);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            var dto = _mapper.Map<ProductDto>(product);
            return Ok(dto);
        }

        // POST: api/Products
        [HttpPost]
        public IActionResult Post([FromBody] ProductCreateDto dto)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
            if (category == null)
            {
                return BadRequest($"Category with ID {dto.CategoryId} does not exist.");
            }

            var product = _mapper.Map<Product>(dto);
            product.CategoryId = dto.CategoryId;

            _context.Products.Add(product);
            _context.SaveChanges();

            var result = _mapper.Map<ProductDto>(product);
            return Ok(result);
        }


        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Znajdź kategorię po nazwie
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

            if (category == null)
            {
                return BadRequest("Invalid category name");
            }

            // Aktualizacja pól
            product.Title = dto.Title;
            product.Author = dto.Author;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Category = category;

            await _context.SaveChangesAsync();

            // Zwróć tylko zmodyfikowane dane
            var result = new
            {
                productId = product.Id,
                product.Title,
                product.Author,
                product.Description,
                product.Price,
                categoryName = product.Category.Name
            };

            return Ok(result);
        }


        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();
            return Ok();
        }
    }
}
