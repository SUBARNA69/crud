using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD2.Models
{
    public class CartModel
    {
        [Key]
        public Guid CartId { get; set; } // Unique identifier for the cart item

        [ForeignKey("Product")]
        public Guid ProductId { get; set; } // Foreign key referencing the ProductModel

        public int Quantity { get; set; }

        public float Price { get; set; }

        // Navigation property to fetch product details
        public ProductModel Product { get; set; }
    }
}
