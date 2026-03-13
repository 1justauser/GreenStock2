namespace GreenStock.Models
{
    public class ShipmentItem
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
