namespace DeliveryService.Models
{
    public class Courier
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Transport { get; set; } = string.Empty;
    }

    public class Order
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "Ожидает";
        public int CourierId { get; set; }
    }

    public class OrderDisplay
    {
        public int OrderId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CourierId { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public string CourierTransport { get; set; } = string.Empty;
    }
}