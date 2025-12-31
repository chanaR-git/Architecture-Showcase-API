namespace Chinese_sale_api.Models
{
    public class Basket
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public int GiftId { get; set; }
        public Gift? gift { get; set; }

        public int amount { get; set; } = 1;

    }
}

