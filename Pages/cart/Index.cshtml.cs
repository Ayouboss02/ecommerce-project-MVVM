using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SleekClothing.Data;
using SleekClothing.Helpers;
using SleekClothing.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SleekClothing.Pages.cart
{
    public class IndexModel : PageModel
    {
        private readonly SleekClothing.Data.ApplicationDbContext _context;

        public string CartTotal { get; set; }

        public IList<Product> Products { get; set; } = default!;

        public IndexModel(SleekClothing.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Utiliser les cookies ou la session pour gérer le panier
            Products = HttpContext.Session.Get<List<Product>>("CartProducts") ?? new List<Product>();
            CartTotal = Products.Sum(p => p.PriceAfterDiscount * p.CartQuantity).ToString("c2");
        }

        public IActionResult OnPostAddToCart(int productId)
        {
            var product = _context.Products.First(x => x.Id == productId);

            // Ajouter au panier via les cookies ou la session
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

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveFromCart(int productId)
        {
            var product = _context.Products.First(x => x.Id == productId);

            // Retirer du panier via les cookies ou la session
            var cartProducts = HttpContext.Session.Get<List<Product>>("CartProducts");

            var productInCart = cartProducts?.FirstOrDefault(p => p.Id == productId);
            if (productInCart != null && productInCart.CartQuantity > 0)
            {
                productInCart.CartQuantity--;
                if (productInCart.CartQuantity == 0)
                {
                    cartProducts.Remove(productInCart);
                }

                HttpContext.Session.Set("CartProducts", cartProducts);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteFromCart(int productId)
        {
            // Récupérer les produits dans le panier
            var cartProducts = HttpContext.Session.Get<List<Product>>("CartProducts") ?? new List<Product>();

            // Trouver le produit à supprimer
            var productToRemove = cartProducts.FirstOrDefault(p => p.Id == productId);

            if (productToRemove != null)
            {
                // Si le produit est trouvé, on le retire
                cartProducts.Remove(productToRemove);
                TempData["success"] = $"{productToRemove.Name} has been removed from your cart.";
            }
            else
            {
                TempData["error"] = "Product not found in the cart.";
            }

            // Mettre à jour la session avec le panier modifié
            HttpContext.Session.Set("CartProducts", cartProducts);

            return RedirectToPage(); // Rediriger vers la même page pour actualiser l'affichage
        }

    }
}
