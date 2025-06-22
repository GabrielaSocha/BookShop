namespace BookShoptry.Models
{
    public class Cart : BaseModel
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public ICollection<CartItem> Items { get; set; }
    }
}
