using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShoptry.Models
{
    public class Product : BaseModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

    }
}
