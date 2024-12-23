namespace CRUD2.Models
{
    public class EditProductModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public string? Image { get; set; }
    }
}
