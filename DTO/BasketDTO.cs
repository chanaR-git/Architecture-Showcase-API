using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class CreateBasketDto
    {
        public int amount { get; set; } = 1;

        [Required]
        public int UserId { get; set; }
        [Required]
        public int GiftId { get; set; }

    }
    public class ReadBasketDto
    {
        public int Id { get; set; }
        public int amount { get; set; } = 1;
        public int UserId { get; set; }

        public required ReadUserDto user { get; set; }

        public int GiftId { get; set; }

        public required ReadGiftDTO gift { get; set; }
    }
}
