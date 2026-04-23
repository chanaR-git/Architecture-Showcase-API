using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.Models
{
    public enum CustomerRole
    {
        User,
        Admin
    }
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        //hashed pwd
        public required string Password { get; set; }
        
        [EmailAddress]
        public required string Email { get; set; }
        
        [Phone]
        public required string Phone { get; set; }

        public List<Gift>? WonGifts { get; set; }
        public List<Purchase>? Purchases { get; set; }

        public required CustomerRole Role { get; set; }
    }
}
