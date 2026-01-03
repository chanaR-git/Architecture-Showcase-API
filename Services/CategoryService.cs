using Chinese_sale_api.Models;
using projectApiAngular.Repositories;
using static Chinese_sale_api.DTO.CategoryDTO;

namespace Chinese_sale_api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;
        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        //get
        public async Task<IEnumerable<ReadCategoryDto>> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllCategories();
            
            if (categories == null)
            {
                _logger.LogWarning("Category Repository returned null");
                return Enumerable.Empty<ReadCategoryDto>();
            }
            var dtos = categories.Select(c => new ReadCategoryDto { Id = c.Id, Name = c.Name });
            _logger.LogDebug("\"Retrieved {Count} categories\"\r\n");
            return dtos;

        }
        //post
        public async Task<ReadCategoryDto> AddCategory(CreateCategoryDto category)
        {
            _logger.LogInformation("Adding new category with name {Name}", category.Name);
            var entity = new Category
            {
                Name = category.Name
            };
            try
            {
               
                var addedCategory = await _categoryRepository.AddCategory(entity);
                _logger.LogInformation("added category {Name} id {Id} succesfully",addedCategory.Name, addedCategory.Id);
                return new ReadCategoryDto { Id = addedCategory.Id, Name = addedCategory.Name };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Failed to add category with name {Name}", category.Name);
                throw;
            }
        }

        //update
        public async Task<ReadCategoryDto?> UpdateCategory(int id, UpdateCategoryDto category)
        {
            _logger.LogInformation("Updating category {Id}",id);
            var entity = new Category
            {
                Name = category.Name
            };
           
            var updatedCategory = await _categoryRepository.UpdateCategory(id, entity);
            if (updatedCategory == null)
            {
                _logger.LogWarning("category {Id} was not found", id);
                return null;
            }
            _logger.LogInformation("category {Id} updated to {Name}", updatedCategory.Id, updatedCategory.Name);
            return new ReadCategoryDto { Id = updatedCategory.Id, Name = updatedCategory.Name };
        }
        //delete
        public async Task<ReadCategoryDto?> DeleteCategory(int id)
        {
            _logger.LogInformation("Deleting category: {id}",id);
            var deletedCategory = await _categoryRepository.DeleteCategory(id);
            if (deletedCategory == null)
            {
                _logger.LogWarning("category {id} was not found",id);
                return null;
            }
            _logger.LogInformation("deleted category {id}",id);
            return new ReadCategoryDto { Id = deletedCategory.Id, Name = deletedCategory.Name };
        }

    }
}
