using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        // Contexte de la base de données, utilisé pour interagir avec les données.
        private readonly SleekClothing.Data.ApplicationDbContext _context;

        // Constructeur de la classe IndexModel.
        public IndexModel(SleekClothing.Data.ApplicationDbContext context)
        {
            _context = context; // Initialisation du contexte de base de données.
        }

        // Propriété pour stocker le terme de recherche saisi par l'utilisateur.
        [BindProperty]
        public string SearchTerm { get; set; }

        // Propriété pour stocker l'ID de la catégorie sélectionnée par l'utilisateur.
        [BindProperty]
        public int? SelectedCategoryId { get; set; } // ID de la catégorie sélectionnée

        // Liste des produits à afficher sur la page.
        public IList<Product> Products { get; set; }

        // Liste des catégories disponibles.
        public IList<Category> Categories { get; set; } // Liste des catégories

        // Méthode appelée lors d'une requête GET pour charger les produits et les catégories.
        public async Task OnGetAsync()
        {
            // Charger toutes les catégories depuis la base de données.
            Categories = await _context.Categories.ToListAsync();

            // Charger tous les produits depuis la base de données par défaut.
            Products = await _context.Products.ToListAsync();
        }

        // Méthode appelée lors d'une requête POST pour réinitialiser les filtres de recherche.
        public IActionResult OnPostResetSearch()
        {
            // Rediriger vers la page des produits sans filtres appliqués.
            return Redirect("/products");
        }

        // Méthode appelée lors d'une requête POST pour rechercher des produits.
        public async Task<IActionResult> OnPostSearchAsync()
        {
            // Charger les catégories pour permettre à la vue de les afficher.
            Categories = await _context.Categories.ToListAsync();

            // Vérifier si un terme de recherche ou une catégorie a été sélectionnée.
            if (string.IsNullOrWhiteSpace(SearchTerm) && !SelectedCategoryId.HasValue)
            {
                TempData["info"] = "Please enter a valid search term or select a category.";
                // Rediriger avec un message d'information si aucune recherche n'est effectuée.
                return Redirect("/products");
            }

            // Créer une requête pour les produits.
            var query = _context.Products.AsQueryable();

            // Filtrer les produits par le terme de recherche si fourni.
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(SearchTerm) || x.Category.Name.Contains(SearchTerm));
            }

            // Filtrer les produits par la catégorie sélectionnée si fournie.
            if (SelectedCategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == SelectedCategoryId.Value);
            }

            // Récupérer la liste des produits filtrés selon les critères de recherche.
            Products = await query.ToListAsync();

            // Afficher la page avec les résultats filtrés.
            return Page();
        }

        // Méthode appelée lors d'une requête POST pour ajouter un produit au panier.
        public IActionResult OnPostAddToCart(int productId)
        {
            // Charger tous les produits depuis la base de données.
            Products = _context.Products.ToList();
            // Trouver le produit à ajouter au panier par son ID.
            Product product = _context.Products.FirstOrDefault(x => x.Id == productId);

            // Vérifier si le produit existe.
            if (product == null)
            {
                TempData["error"] = "Product not found.";
                // Rediriger vers la page des produits avec un message d'erreur si le produit n'existe pas.
                return Redirect("/products");
            }

            // Vérifier si le produit est en rupture de stock.
            if (product.IsOutOfStock)
            {
                TempData["error"] = $"{product.Name} is out of stock.";
                // Rediriger vers la page des produits avec un message d'erreur si le produit est en rupture de stock.
                return Redirect("/products");
            }

            // Ajouter le produit au panier via les sessions (ou cookies).
            var cartProducts = HttpContext.Session.Get<List<Product>>("CartProducts") ?? new List<Product>();

            // Vérifier si le produit est déjà dans le panier.
            var existingProduct = cartProducts.FirstOrDefault(p => p.Id == productId);
            if (existingProduct != null)
            {
                // Si le produit existe déjà, augmenter la quantité.
                existingProduct.CartQuantity++;
            }
            else
            {
                // Si le produit n'est pas dans le panier, l'ajouter avec une quantité de 1.
                product.CartQuantity = 1;
                cartProducts.Add(product);
            }

            // Mettre à jour le panier dans la session.
            HttpContext.Session.Set("CartProducts", cartProducts);

            // Afficher un message de succès à l'utilisateur.
            TempData["success"] = $"{product.Name} added to cart successfully!";
            // Rediriger vers la page des produits.
            return Redirect("/products");
        }
    }
}
