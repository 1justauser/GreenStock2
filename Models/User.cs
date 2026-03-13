namespace GreenStock.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" or "Kladovshik"

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
