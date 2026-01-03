using Chinese_sale_api.DTO;
using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chinese_sale_api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _service;
        public BasketController(IBasketService service)
        {
            _service = service;
        }
        [HttpGet("myBasket")]
        public async Task<IActionResult> GetMyBasketAsync()
        {
            try
            {
                var items = await _service.GetMyBasket();
                return Ok(items);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnterToBasketAsync([FromBody] CreateBasketDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _service.EnterToBasketAsync(dto);
                return Created(string.Empty, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}/amount")]
        public async Task<IActionResult> UpdateBasketAmountAsync([FromRoute] int id, [FromQuery] int newAmount)
        {
            if (id <= 0) return BadRequest("Invalid id");

            try
            {
                var updated = await _service.UpdateBasketAmountAsync(id, newAmount);
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest("newAmount must be between allowed range");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

S        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBasketAsync([FromRoute] int id)
        {
            if (id <= 0) return BadRequest("Invalid id");

            try
            {
                var deleted = await _service.DeleteBasketAsync(id);
                return deleted is null ? NotFound() : Ok(deleted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
