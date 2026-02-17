using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using projectApiAngular.Repositories;
using static Chinese_sale_api.DTO.CategoryDTO;

namespace Chinese_sale_api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;
        private const string ClassName = nameof(CategoryService);

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        //get
        public async Task<IEnumerable<ReadCategoryDto>> GetAllCategories()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetAllCategories), ClassName);
            var categories = await _categoryRepository.GetAllCategories();
            
            if (categories == null)
            {
                LoggingHelper.LogValidationError(_logger, nameof(GetAllCategories), ClassName, "Repository returned null");
                return Enumerable.Empty<ReadCategoryDto>();
            }
            var dtos = categories.Select(c => new ReadCategoryDto { Id = c.Id, Name = c.Name });
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetAllCategories), ClassName, dtos.Count());
            return dtos;
        }

        //post
        public async Task<ReadCategoryDto> AddCategory(CreateCategoryDto category)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(AddCategory), ClassName, new { category.Name });
            var entity = new Category
            {
                Name = category.Name
            };
            try
            {
                var addedCategory = await _categoryRepository.AddCategory(entity);
                LoggingHelper.LogCreated(_logger, nameof(AddCategory), ClassName, new { addedCategory.Id, addedCategory.Name });
                return new ReadCategoryDto { Id = addedCategory.Id, Name = addedCategory.Name };
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(AddCategory), ClassName, ex);
                throw;
            }
        }

        //update
        public async Task<ReadCategoryDto?> UpdateCategory(int id, UpdateCategoryDto category)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(UpdateCategory), ClassName, new { CategoryId = id, NewName = category.Name });
            var entity = new Category
            {
                Name = category.Name
            };
           
            var updatedCategory = await _categoryRepository.UpdateCategory(id, entity);
            if (updatedCategory == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(UpdateCategory), ClassName, id.ToString());
                return null;
            }
            LoggingHelper.LogUpdated(_logger, nameof(UpdateCategory), ClassName, new { updatedCategory.Id, updatedCategory.Name });
            return new ReadCategoryDto { Id = updatedCategory.Id, Name = updatedCategory.Name };
        }

        //delete
        public async Task<ReadCategoryDto?> DeleteCategory(int id)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(DeleteCategory), ClassName, new { CategoryId = id });
            var deletedCategory = await _categoryRepository.DeleteCategory(id);
            if (deletedCategory == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(DeleteCategory), ClassName, id.ToString());
                return null;
            }
            LoggingHelper.LogDeleted(_logger, nameof(DeleteCategory), ClassName, id.ToString());
            return new ReadCategoryDto { Id = deletedCategory.Id, Name = deletedCategory.Name };
        }

    }
}
