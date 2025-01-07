using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SleekClothing.Helpers;
using SleekClothing.Models;
using System.Linq;

namespace SleekClothing.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SleekClothing.Data.ApplicationDbContext _context;

        public IndexModel(SleekClothing.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Products != null)
            {
                Products = await _context.Products.ToListAsync(); // get from db
                Products = Products.OrderByDescending(x => x.DateCreated).Take(8).ToList(); // order by newest
            }
        }

        public IActionResult OnPostAddToCart(int productId)
        {
            Products = _context.Products.ToList();
            Product product = _context.Products.First(x => x.Id == productId);

            // Handle product out of stock
            if (product.IsOutOfStock)
            {
                TempData["error"] = $"{product.Name} is out of stock.";
                return Redirect("/");
            }

            // Ajout au panier via les sessions (ou cookies)
            var cartProducts = HttpContext.Session.Get<List<Product>>("CartProducts") ?? new List<Product>();

            var existingProduct = cartProducts.FirstOrDefault(p => p.Id == productId);
            if (existingProduct != null)
            {
                existingProduct.CartQuantity++;
            }
            else
            {
                product.CartQuantity = 1;
                cartProducts.Add(product);
            }

            HttpContext.Session.Set("CartProducts", cartProducts);

            TempData["success"] = $"{product.Name} added to cart successfully!";
            return Redirect("/");
        }
    }
}