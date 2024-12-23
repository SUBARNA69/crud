namespace CRUD2.Models
{
    public class AddProductModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public IFormFile? photo { get; set; }

    }
}
