using System;
using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTOs
{
    public class CreatePurchaseDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int GiftId { get; set; }

        // optional: if not provided, service will set DateTime.UtcNow
        public DateTime? PurchDate { get; set; }
    }
}