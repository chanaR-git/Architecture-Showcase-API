using Chinese_sale_api.Models;

namespace Chinese_sale_api.DTO
{
    public class ReadGiftDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string CategoryName { get; set; }
        public required string DonorName { get; set; }
        public int Price { get; set; } = 10;
        public required string ImagePath { get; set; }

    }
    public class CreateGiftDTO
    {
        public required string Name { get; set; }
        public  required string Description { get; set; }
        public required int CategoryId { get; set; }
        public required int DonorId { get; set; }
        public int Price { get; set; } = 10;
        public required string ImagePath { get; set; }
    }
    public class UpdateGiftDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; } 
        public int? CategoryId { get; set; }
        public int? DonorId { get; set; }
        public int? Price { get; set; }
        public string? ImagePath { get; set; }

    }
}
