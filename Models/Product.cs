namespace GreenStock.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Article { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string Unit { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public int Stock { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    }
}
