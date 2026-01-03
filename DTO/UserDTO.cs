using Chinese_sale_api.Models;
using Chinese_sale_api.Validations;
using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }
       
        [StrongPassword]
        public required string Password { get; set; }
        [EmailAddress]
        [Required]
        [MaxLength(50)]
        public required string Email { get; set; }
        [Phone]
        [Required]
        [MaxLength(15)]
        public required string Phone { get; set; }
    }
    public class ReadUserDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [Phone]
        public required string Phone { get; set; }
        public string Role { get; set; } = CustomerRole.User.ToString();
    }
    public class LoginDTO
    {
        [StrongPassword]
        public required string Password { get; set; }
        [EmailAddress]
        [Required]
        [MaxLength(50)]
        public required string Email { get; set; }
    }
}
