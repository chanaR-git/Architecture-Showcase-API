using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface ICategoryService
    {
        Task<CategoryDTO.ReadCategoryDto> AddCategory(CategoryDTO.CreateCategoryDto category);
        Task<CategoryDTO.ReadCategoryDto> DeleteCategory(int id);
        Task<IEnumerable<CategoryDTO.ReadCategoryDto>> GetAllCategories();
        Task<CategoryDTO.ReadCategoryDto> UpdateCategory(int id, CategoryDTO.CreateCategoryDto category);
    }
}