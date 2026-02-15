using Chinese_sale_api.DTO;
using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chinese_sale_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftController : ControllerBase
    {
        private readonly IGiftService _service;
        public GiftController(IGiftService srv)
        {
            _service = srv;
        }

        [HttpGet]
        public async Task<IActionResult> GetGiftsAsync()
        {
            var res = await _service.GetGiftsAsync();
            return Ok(res);
        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetGiftByName([FromRoute] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("name is required");

            var res = await _service.GetGiftByNameAsync(name);
            return res is null ? NotFound() : Ok(res);
        }

        [HttpGet("bynumberofbuyers/{num}")]
        public async Task<IActionResult> getGiftByNumBuyersAsync([FromRoute] int num)
        {
            if (num < 0)
                return BadRequest("num must be >= 0");

            var res = await _service.getByNumBuyersAsync(num);
            return Ok(res);
        }
        [HttpGet("bydonor/{name}")]
        public async Task<IActionResult> GetGiftByDonorAsync([FromRoute] string name)
        {
            var res = await _service.GetGiftByDonorAsync(name);
            return Ok(res);
        }

        [HttpGet("{giftName}/winner")]
        public async Task<IActionResult> GetGiftWinnerAsync([FromRoute] string giftName)
        {
            var winner = await _service.GetWinnerOfGift(giftName);
            return winner is null ? NotFound() : Ok(new {winner});
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGiftAsync([FromBody] CreateGiftDTO g)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var res = await _service.AddGiftAsync(g);
                return Ok(res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // 409
            }

        }
        [HttpPut("{name}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGiftAsync([FromRoute] string name, [FromBody] UpdateGiftDTO updatedGift)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var res = await _service.UpdateGiftAsync(name, updatedGift);
                if (res is null)
                    return NotFound();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{name}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGiftAsync([FromRoute] string name)
        {
            var res = await _service.DeleteGiftAsync(name);
            return res is null ? NotFound() :  Ok(res);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetGiftsPagedAsync([FromQuery] PaginationParams @params)
        {
            // וולידציה בסיסית
            if (@params.PageNumber < 1 || @params.PageSize < 1)
                return BadRequest("PageNumber and PageSize must be greater than 0");

            var res = await _service.GetGiftsPagedAsync(@params);
            return Ok(res);
        }

        [HttpPost("{giftId}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadGiftImageAsync([FromRoute] int giftId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            var result = await _service.UploadGiftImageAsync(giftId, file);
            return Ok(result);
        }

        [HttpGet("{giftId}/image")]
        [Authorize]
        public async Task<IActionResult> DownloadGiftImageAsync([FromRoute] int giftId)
        {
            var fileStream = await _service.DownloadGiftImageAsync(giftId);
            return File(fileStream, "image/jpeg", "gift-image.jpg");
        }
    }
}
