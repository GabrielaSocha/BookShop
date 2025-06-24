using BookShoptry.Data;
using BookShoptry.Models;

namespace BookShoptry.Seeding
{
    public static class DbSeeder
    {
        public static void Seed(StoreContext context)
        {
            if (!context.Categories.Any())
            {
                var fantasy = new Category { Name = "Fantasy" };
                var science = new Category { Name = "Science" };
                var history = new Category { Name = "History" };
                var biography = new Category { Name = "Biography" };
                var technology = new Category { Name = "Technology" };

                context.Categories.AddRange(fantasy, science, history, biography, technology);
                context.SaveChanges();

                context.Products.AddRange(
                    new Product
                    {
                        Title = "Władca Pierścieni",
                        Author = "J.R.R. Tolkien",
                        Description = "Epicka powieść fantasy",
                        Price = 49.99m,
                        Stock = 15,
                        CategoryId = fantasy.Id
                    },
                    new Product
                    {
                        Title = "Krótka historia czasu",
                        Author = "Stephen Hawking",
                        Description = "Klasyczna pozycja naukowa",
                        Price = 39.99m,
                        Stock = 10,
                        CategoryId = science.Id
                    },
                    new Product
                    {
                        Title = "Zbrodnia i kara",
                        Author = "Fiodor Dostojewski",
                        Description = "Klasyczna powieść psychologiczna",
                        Price = 29.99m,
                        Stock = 12,
                        CategoryId = fantasy.Id
                    }
                );

                context.SaveChanges();
            }

            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { Username = "Anna Kowalska", Email = "anna@example.com", Role = "User"},
                    new Customer { Username = "Jan Nowak", Email = "jan@example.com", Role = "User" },
                    new Customer { Username = "Anna Kowalska", Email = "anna@example.com", Role = "User" },
                    new Customer { Username = "Jan Nowak", Email = "jan@example.com", Role = "User" },
                    new Customer { Username = "Piotr Zieliński", Email = "piotr@example.com", Role = "User" },
                    new Customer { Username = "Katarzyna Wiśniewska", Email = "kasia@example.com", Role = "User" },
                    new Customer { Username = "Tomasz Wójcik", Email = "tomasz@example.com", Role = "User" },
                    new Customer { Username = "Magdalena Lewandowska", Email = "magda@example.com", Role = "User" },
                    new Customer { Username = "Marek Dąbrowski", Email = "marek@example.com", Role = "User" }
                );
                context.SaveChanges();
            }
        }
    }
}
