using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SleekClothing.Data;
using SleekClothing.Helpers;
using SleekClothing.Models;

namespace SleekClothing.Pages.products
{
    public class IndexModel : PageModel
    {
        private readonly SleekClothing.Data.ApplicationDbContext _context;

        public IndexModel(SleekClothing.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string SearchTerm { get; set; }

        [BindProperty]
        public int? SelectedCategoryId { get; set; } // ID de la catégorie sélectionnée

        public IList<Product> Products { get; set; }

        public IList<Category> Categories { get; set; } // Liste des catégories

        public async Task OnGetAsync()
        {
            if (_context.Products != null)
            {
                // Charger les catégories
                Categories = await _context.Categories.ToListAsync();

                // Charger tous les produits par défaut
                Products = await _context.Products.ToListAsync();
            }
        }

        public IActionResult OnPostResetSearch()
        {
            return Redirect("/products");
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm) && !SelectedCategoryId.HasValue)
            {
                TempData["info"] = $"Please enter a valid search term or select a category.";
                return Redirect("/products");
            }

            var query = _context.Products.AsQueryable();

            // Filtrer par terme de recherche
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(SearchTerm) || x.Category.Name.Contains(SearchTerm));
            }

            // Filtrer par catégorie
            if (SelectedCategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == SelectedCategoryId.Value);
            }

            Products = await query.ToListAsync();

            return Page();
        }

        public IActionResult OnPostAddToCart(int productId)
{
    try
    {
        Products = _context.Products.ToList();

        Product product = _context.Products.First(x => x.Id == productId);

        // Gérer le produit en rupture de stock
        if (product.IsOutOfStock)
        {
            TempData["error"] = $"{product.Name} is out of stock.";
            return Redirect("/products");
        }

        // Supprimer le troisième argument dans AddToCartDb
        CartHelper.AddToCartDb(product, _context);

        TempData["success"] = $"{product.Name} added to cart successfully!";
        return Redirect("/products");
    }
    catch
    {
        TempData["error"] = $"Login to add items to your cart.";
        return Redirect("/products");
    }
}

    }
}
