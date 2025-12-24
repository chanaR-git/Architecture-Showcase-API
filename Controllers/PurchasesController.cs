using Chinese_sale_api.DTO;
using Chinese_sale_api.DTOs;
using Chinese_sale_api.Services;
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
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetBuyersDetailsAsync()
        {
            var items = await _service.GetBuyersDetailsAsync();
            return Ok(items);
        }

        [HttpGet("gift/{name}")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetByGift(string name)
        {
            var items = await _service.GetPurchasesByGiftAsync(name);
            return Ok(items);
        }

        [HttpGet("sorted/sellings")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetSortedBySellings()
        {
            var items = await _service.GetPurchasesSortedBySellingsAsync();
            return Ok(items);
        }

        [HttpGet("sorted/price")]
        public async Task<ActionResult<IEnumerable<ReadPurchaseDto>>> GetSortedByPrice()
        {
            var items = await _service.GetPurchasesSortedByPriceAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult<ReadPurchaseDto>> Create([FromBody] CreatePurchaseDto dto)
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