using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //hashed pwd
        public string Password { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }

        public List<Gift> WonGifts { get; set; }
        public List<Purchase> Purchases { get; set; }

    }
}
