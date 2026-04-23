
using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;


namespace projectApiAngular.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ChineseSaleDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;
        private const string ClassName = nameof(CategoryRepository);

        public CategoryRepository(ChineseSaleDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //get 
        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetAllCategories), ClassName);
                var categories = await _context.Categories.ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetAllCategories), ClassName, categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetAllCategories), ClassName, ex);
                throw;
            }
        }

        public async Task<Category?> GetCategoryById(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetCategoryById), ClassName, new { CategoryId = id });
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetCategoryById), ClassName, id.ToString());
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetCategoryById), ClassName);
                return category;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetCategoryById), ClassName, ex);
                throw;
            }
        }

        //post
        public async Task<Category> AddCategory(Category category)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(AddCategory), ClassName, new { category.Name });
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                LoggingHelper.LogCreated(_logger, nameof(AddCategory), ClassName, new { category.Id, category.Name });
                return category;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(AddCategory), ClassName, ex);
                throw;
            }
        }

        //update
        public async Task<Category?> UpdateCategory(int id, Category category)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(UpdateCategory), ClassName, new { CategoryId = id });
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(UpdateCategory), ClassName, id.ToString());
                    return null;
                }
                existingCategory.Name = category.Name;
                await _context.SaveChangesAsync();
                LoggingHelper.LogUpdated(_logger, nameof(UpdateCategory), ClassName, new { id, category.Name });
                return existingCategory;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(UpdateCategory), ClassName, ex);
                throw;
            }
        }

        //delete
        public async Task<Category?> DeleteCategory(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(DeleteCategory), ClassName, new { CategoryId = id });
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(DeleteCategory), ClassName, id.ToString());
                    return null;
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                LoggingHelper.LogDeleted(_logger, nameof(DeleteCategory), ClassName, id.ToString());
                return category;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(DeleteCategory), ClassName, ex);
                throw;
            }
        }
    }
}

