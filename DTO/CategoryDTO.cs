using System.ComponentModel.DataAnnotations;

namespace Chinese_sale_api.DTO
{
    public class CategoryDTO
    {
        public class CreateCategoryDto
        {
            [Required]
            [MaxLength(50)]
            public required string Name { get; set; }
        }
        public class UpdateCategoryDto
        {
            [MaxLength(50)]
            public string? Name { get; set; }
        }
        public class ReadCategoryDto
        {
            public int Id { get; set; }
            public required string Name { get; set; }
        }
    }
}
