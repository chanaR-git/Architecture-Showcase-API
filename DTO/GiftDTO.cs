using Chinese_sale_api.Models;
using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class ReadGiftDTO
    {

        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public required string DonorName { get; set; }
        public int DonorId { get; set; }
        public int Price { get; set; } = 10;
        public required string ImagePath { get; set; }

    }
    public class CreateGiftDTO
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        [Required]
        [MaxLength(200)]
        public  required string Description { get; set; }
        [Required]
        public required int CategoryId { get; set; }
        [Required]
        public required int DonorId { get; set; }
        [Required]
        public int Price { get; set; } = 10;
        [Required]
        [MaxLength(200)]
        public required string ImagePath { get; set; }
    }
    public class UpdateGiftDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        [MaxLength(200)]
        public string? Description { get; set; } 
        public int? CategoryId { get; set; }
        public int? DonorId { get; set; }
        public int? Price { get; set; }
        [MaxLength(200)]
        public string? ImagePath { get; set; }

    }
}
