using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chinese_sale_api.DTOs
{
    public class ReadPurchaseDto
    {
        public required int Id { get; set; }
        public required int CustomerId { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required int GiftId { get; set; }
        public required string GiftName { get; set; }
        public required int GiftPrice { get; set; }
        public required DateTime PurchDate { get; set; }
    }
    public class CreatePurchaseDto
    {
        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        [ForeignKey("Gift")]
        public int GiftId { get; set; }

        public DateTime PurchDate { get; set; } = DateTime.Now;
    }
}