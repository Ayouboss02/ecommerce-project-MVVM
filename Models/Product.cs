using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SleekClothing.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(8)")]
        public string SKU { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public int Discount { get; set; }

        public DateTime DateCreated { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ImageLocation { get; set; }

        #region not included in EF migrations

        // Propriété calculée pour le prix après réduction
        [NotMapped]
        public decimal PriceAfterDiscount
        {
            get => Price - (Price * Discount / 100);
        }

        [NotMapped]
        public int CartQuantity { get; set; }

        [NotMapped]
        public bool IsOutOfStock
        {
            get { return Quantity == 0; }
        }

        [NotMapped]
        public bool HasDiscount
        {
            get { return Discount > 0; }
        }

        #endregion
    }
}
