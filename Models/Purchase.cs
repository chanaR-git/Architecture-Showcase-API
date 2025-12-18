namespace Chinese_sale_api.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        
        //customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int GiftId { get; set; }
        public Gift Gift { get; set; }

        public DateTime PurchDate { get; set; }
    }
}
