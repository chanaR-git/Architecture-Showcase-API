using System;

namespace Chinese_sale_api.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }

        public int GiftId { get; set; }
        public string? GiftName { get; set; }
        public int? GiftPrice { get; set; }

        public DateTime PurchDate { get; set; }
    }
}