using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class CreateBasketDto
    {
        public int amount { get; set; } = 1;
        [Required]
        public int GiftId { get; set; }

    }
    public class ReadBasketDto
    {
        public int Id { get; set; }
        public int amount { get; set; } = 1;
        public int UserId { get; set; }

        public  ReadUserDto? user { get; set; }

        public int GiftId { get; set; }

        public  ReadGiftDTO? gift { get; set; }
    }
}
