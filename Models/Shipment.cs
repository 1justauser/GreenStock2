namespace GreenStock.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Recipient { get; set; } = string.Empty;

        public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    }
}
