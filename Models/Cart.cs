using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SleekClothing.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
