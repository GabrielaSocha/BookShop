using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookShoptry.Dtos
{
    public class ProductCreateDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
