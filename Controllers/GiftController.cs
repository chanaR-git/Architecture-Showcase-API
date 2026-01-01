using Chinese_sale_api.DTO;
using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
            var res = await _service.GetGiftByNameAsync(name);
            return res is null ? NotFound() : Ok(res);
        }

        [HttpGet("bynumberofbuyers/{num}")]
        public async Task<IActionResult> getGiftByNumBuyersAsync([FromRoute] int num)
        {
            var res = await _service.getByNumBuyersAsync(num);
            return Ok(res);
        }
        [HttpGet("bydonor/{donorId}")]
        public async Task<IActionResult> GetGiftByDonorAsync([FromRoute] string name)
        {
            var res = await _service.GetGiftByDonorAsync(name);
            return res is null ? NotFound() : Ok(res);
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGiftAsync([FromRoute] string name, [FromBody] UpdateGiftDTO updatedGift)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var res = await _service.UpdateGiftAsync(name, updatedGift);
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
    }
}
