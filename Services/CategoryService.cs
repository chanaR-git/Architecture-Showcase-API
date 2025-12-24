using Chinese_sale_api.Models;
using projectApiAngular.Repositories;
using static Chinese_sale_api.DTO.CategoryDTO;

namespace Chinese_sale_api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        //get
        public async Task<IEnumerable<ReadCategoryDto>> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllCategories();
            if (categories == null) return Enumerable.Empty<ReadCategoryDto>();
            var dtos = categories.Select(c => new ReadCategoryDto { Id = c.Id, Name = c.Name });
            return dtos;

        }
        //post
        public async Task<ReadCategoryDto> AddCategory(CreateCategoryDto category)
        {

            var entity = new Category
            {
                Name = category.Name
            };
            var addedCategory = await _categoryRepository.AddCategory(entity);
            return new ReadCategoryDto { Id = addedCategory.Id, Name = addedCategory.Name };
        }

        //update
        public async Task<ReadCategoryDto?> UpdateCategory(int id, UpdateCategoryDto category)
        {
            var entity = new Category
            {
                Name = category.Name
            };
            var updatedCategory = await _categoryRepository.UpdateCategory(id, entity);
            if (updatedCategory == null) return null;
            return new ReadCategoryDto { Id = updatedCategory.Id, Name = updatedCategory.Name };
        }
        //delete
        public async Task<ReadCategoryDto> DeleteCategory(int id)
        {
            var deletedCategory = await _categoryRepository.DeleteCategory(id);
            if (deletedCategory == null) return null;
            return new ReadCategoryDto { Id = deletedCategory.Id, Name = deletedCategory.Name };
        }

    }
}
