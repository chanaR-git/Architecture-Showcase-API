using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.Models
{
    public class Manager
    {
        public int Id { get; set; }
        //hashed pwd
        public string Password { get; set; }
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }
}
