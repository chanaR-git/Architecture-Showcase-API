using Chinese_sale_api.Models;

namespace projectApiAngular.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category> AddCategory(Category category);
        Task<Category?> DeleteCategory(int id);
        Task<IEnumerable<Category>> GetAllCategories();
        Task<Category?> UpdateCategory(int id, Category category);
    }
}