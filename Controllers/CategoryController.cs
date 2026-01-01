using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Chinese_sale_api.DTO.CategoryDTO;

namespace Chinese_sale_api.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        //get
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var gifts = await _categoryService.GetAllCategories();
            return Ok(gifts);
        }
        
        //post
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDto category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var c = await _categoryService.AddCategory(category);
                return Ok(c);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var deletedCategory = await _categoryService.DeleteCategory(id);
                if (deletedCategory == null)
                    return NotFound();
                return Ok(deletedCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        //update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedCategory = await _categoryService.UpdateCategory(id, category);
                if (updatedCategory == null)
                    return NotFound();
                return Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }
    }
}
