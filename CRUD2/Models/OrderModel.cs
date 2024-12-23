using System.ComponentModel.DataAnnotations.Schema;

namespace CRUD2.Models
{
    public class OrderModel
    {
        public Guid Id { get; set; } // Unique identifier for the order

        [ForeignKey("Cart")]
        public Guid CartId { get; set; } // ID of the cart associated with the order

        public int Quantity { get; set; } // Quantity of the product included in the order
        public decimal Price { get; set; } // Price of the product

        // Navigation property to access the associated cart
        public CartModel? Cart { get; set; }

        // You can add more properties as needed, such as UserId, OrderDate, etc.
    }
}
