using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SleekClothing.Data;
using SleekClothing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SleekClothing.Helpers
{
    public class CartHelper
    {
        // Nom du cookie utilisé pour stocker les données du panier
        const string COOKIE_NAME = "SLKCARTDATA";

        #region cookie logic

        // Crée un cookie de panier avec une date d'expiration définie
        private static void CreateCartCookie(HttpContext httpContext)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(240) // Expiration du cookie dans 240 jours
            };

            httpContext.Response.Cookies.Append(COOKIE_NAME, "", cookieOptions);
        }

        // Récupère les articles du panier stockés dans le cookie de l'utilisateur
        public static List<Product> GetUserCartCookie(HttpContext httpContext)
        {
            var cookieValue = httpContext.Request.Cookies[COOKIE_NAME];
            if (cookieValue == null)
            {
                return new List<Product>();
            }

            return JsonConvert.DeserializeObject<List<Product>>(cookieValue) ?? new List<Product>();
        }

        // Ajoute un produit au panier en le stockant dans le cookie
        public static void AddToCartCookie(Product newProduct, HttpContext httpContext)
        {
            List<Product> cartItems = new List<Product>();
            var cookieValue = httpContext.Request.Cookies[COOKIE_NAME];

            // Charger les articles du cookie s'il existe
            if (cookieValue == null)
            {
                CreateCartCookie(httpContext);
            }
            else
            {
                cartItems = JsonConvert.DeserializeObject<List<Product>>(cookieValue);
            }

            // Vérifier si le produit est déjà dans le panier
            var existingProduct = cartItems.FirstOrDefault(p => p.Id == newProduct.Id);
            if (existingProduct != null)
            {
                existingProduct.CartQuantity += 1; // Mise à jour de la quantité si le produit existe
            }
            else
            {
                newProduct.CartQuantity = 1; // Initialisation de la quantité pour un nouveau produit
                cartItems.Add(newProduct); // Ajouter le nouveau produit au panier
            }

            // Mettre à jour le cookie
            var newCookieValue = JsonConvert.SerializeObject(cartItems, Formatting.Indented);
            httpContext.Response.Cookies.Append(COOKIE_NAME, newCookieValue);
        }

        // Supprime un produit du panier stocké dans le cookie
        public static void RemoveFromCartCookie(Product product, HttpContext httpContext)
        {
            var cookieValue = httpContext.Request.Cookies[COOKIE_NAME];
            if (string.IsNullOrEmpty(cookieValue)) return;

            var cartItems = JsonConvert.DeserializeObject<List<Product>>(cookieValue);

            // Supprimer le produit du panier
            cartItems.RemoveAll(item => item.Id == product.Id);

            var newCookieValue = JsonConvert.SerializeObject(cartItems, Formatting.Indented);
            httpContext.Response.Cookies.Append(COOKIE_NAME, newCookieValue);
        }

        // Supprime le cookie de panier
        public static void DeleteCartCookie(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete(COOKIE_NAME);
        }

        // Récupère les articles du panier et les groupe par identifiant de produit
        public static List<Product> GetGroupedCartItemsCookie(HttpRequest httpRequest)
        {
            var cookieValue = httpRequest.Cookies[COOKIE_NAME];
            if (cookieValue == null)
            {
                return new List<Product>(); // retourne une liste vide si le cookie est null
            }

            var products = JsonConvert.DeserializeObject<List<Product>>(cookieValue);

            var groupedProducts = products
                .GroupBy(u => u.Id)
                .Select(group => new Product
                {
                    Id = group.Key,
                    CartQuantity = group.Count(),
                    Name = group.First().Name,
                    Price = group.First().Price,
                    Description = group.First().Description,
                    ImageLocation = group.First().ImageLocation,
                    Category = group.First().Category
                })
                .ToList();

            return groupedProducts;
        }

        // Compte le nombre d'articles dans le panier stocké dans le cookie
        public static int GetCartItemsCountCookie(HttpContext httpContext)
        {
            var cookieValue = httpContext.Request.Cookies[COOKIE_NAME];
            if (cookieValue == null)
            {
                return 0; // retourne 0 si aucun panier n'existe dans le cookie
            }

            var products = JsonConvert.DeserializeObject<List<Product>>(cookieValue);
            return products.Count;
        }

        // Calcule le total du panier en fonction des articles et de leur quantité
        public static decimal GetCartTotalCookie(HttpRequest httpRequest)
        {
            var items = GetGroupedCartItemsCookie(httpRequest);
            return items.Sum(item => item.PriceAfterDiscount * item.CartQuantity);
        }

        #endregion

        #region db logic (using Cart and CartItem models)

        // Ajoute un produit au panier dans la base de données
        public static void AddToCartDb(Product product, ApplicationDbContext context)
        {
            var cart = context.Carts.FirstOrDefault();
            if (cart == null)
            {
                cart = new Cart { CartItems = new List<CartItem>() };
                context.Carts.Add(cart);
            }

            // Vérifier si le produit existe déjà dans le panier
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == product.Id);
            if (cartItem != null)
            {
                cartItem.Quantity += 1; // Mettre à jour la quantité
            }
            else
            {
                cartItem = new CartItem { ProductId = product.Id, Quantity = 1 };
                cart.CartItems.Add(cartItem);
            }

            context.SaveChanges();
        }

        // Vide le panier dans la base de données
        public static void ClearCartDb(ApplicationDbContext context)
        {
            var cart = context.Carts.FirstOrDefault();
            if (cart != null)
            {
                cart.CartItems.Clear();
                context.SaveChanges();
            }
        }

        // Supprime un produit spécifique du panier dans la base de données
        public static void RemoveFromCartDb(Product product, ApplicationDbContext context)
        {
            var cart = context.Carts.FirstOrDefault();
            if (cart != null)
            {
                var itemsToRemove = cart.CartItems.Where(item => item.ProductId == product.Id).ToList();

                foreach (var item in itemsToRemove)
                {
                    cart.CartItems.Remove(item);
                }

                context.SaveChanges();
            }
        }

        // Récupère tous les articles du panier dans la base de données
        public static List<Product> GetCartItemsDb(ApplicationDbContext context)
        {
            var cart = context.Carts.FirstOrDefault();
            if (cart == null) return new List<Product>();

            return cart.CartItems.Select(ci => new Product
            {
                Id = ci.ProductId,
                CartQuantity = ci.Quantity
            }).ToList();
        }

        // Compte le nombre d'articles dans le panier stocké dans la base de données
        public static int GetCartItemsCountDb(ApplicationDbContext context)
        {
            var cart = context.Carts.FirstOrDefault();
            return cart?.CartItems.Count ?? 0;
        }

        // Calcule le total du panier dans la base de données
        public static decimal GetCartTotalDb(ApplicationDbContext context)
        {
            var cart = context.Carts.FirstOrDefault();
            if (cart == null) return 0;

            return cart.CartItems.Sum(item => item.Product.PriceAfterDiscount * item.Quantity);
        }

        #endregion

        #region convert cookie items to db items

        // Convertit les articles du panier stocké dans le cookie en enregistrements de la base de données
        public static void ConvertToDB(HttpContext httpContext, ApplicationDbContext context)
        {
            var cookieItems = GetUserCartCookie(httpContext);

            if (cookieItems.Count == 0) return;

            foreach (var product in cookieItems)
            {
                AddToCartDb(product, context);
            }

            DeleteCartCookie(httpContext);
        }

        #endregion
    }
}
