using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.Models
{
    public class Donor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        [EmailAddress]
        public required string Email { get; set; }
        [Phone]
        public required string Phone { get; set; }
        public List<Gift>? MyGifts { get; set; }
    }
}
