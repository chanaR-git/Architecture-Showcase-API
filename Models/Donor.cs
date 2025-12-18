using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.Models
{
    public class Donor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Gift> MyGifts { get; set; }
    }
}
