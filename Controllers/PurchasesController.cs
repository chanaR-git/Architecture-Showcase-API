using Chinese_sale_api.DTO;
using Chinese_sale_api.DTOs;
using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chinese_sale_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchasesService _service;
        public PurchasesController(IPurchasesService service)
        {
            _service = service;
        }

        [HttpGet("buyers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetBuyersDetailsAsync()
        {
            var items = await _service.GetBuyersDetailsAsync();
            return Ok(items);
        }

        [HttpGet("gift/{name}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetPurchaseByGift(string name)
        {
            var items = await _service.GetPurchasesByGiftAsync(name);
            return Ok(items);
        }

        [HttpGet("sorted/sellings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetPurchasesSortedBySellings()
        {
            var items = await _service.GetPurchasesSortedBySellingsAsync();
            return Ok(items);
        }

        [HttpGet("sorted/price")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetPurchasesSortedByPrice()
        {
            var items = await _service.GetPurchasesSortedByPriceAsync();
            return Ok(items);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ReadPurchaseDto>> Purchase([FromBody] CreatePurchaseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _service.AddPurchaseAsync(dto);
                return Created(string.Empty, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}