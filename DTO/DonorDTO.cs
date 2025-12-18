using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class CreateDonorDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }

    }
    public class UpdateDonorDTO
    {
        
        public string? Name { get; set; }
        
        public string? Email { get; set; }
        
        public string? Phone { get; set; }
    }

      public class ReadDonorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
