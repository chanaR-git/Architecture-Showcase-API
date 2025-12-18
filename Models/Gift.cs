using System.Reflection.Metadata.Ecma335;

namespace Chinese_sale_api.Models
{
    public class Gift
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int DonorId { get; set; }
        public Donor Donor { get; set; }
        public int Price { get; set; }
        public string ImagePath { get; set; }

        public int? WinnerId { get; set; }
        public Customer? Winner { get; set; }

        public List<Purchase> Purchases { get; set; }
                
    }
}
