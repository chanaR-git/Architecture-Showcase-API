using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class CreateDonorDTO
    {
        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [Phone]
        public required string Phone { get; set; }

    }
    public class UpdateDonorDTO
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(15)]
        public string? Phone { get; set; }
    }

      public class ReadDonorDTO
    {
        public required  int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
    }
}
