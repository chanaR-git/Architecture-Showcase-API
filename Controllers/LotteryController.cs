using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chinese_sale_api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        public LotteryController(ILotteryService lotteryService)
        {
            _lotteryService = lotteryService;
        }

        [HttpPost]
        public async Task<IActionResult> RunLottery()
        {            
            try
            {
                var result = await _lotteryService.RunLottery();
                return Ok(result);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("winners")]
        public async Task<IActionResult> GetAllGiftWinners()
        {
            try
            {
                var result = await _lotteryService.GetAllGiftWinners();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("newSale")]
        public async Task<IActionResult> StartNewSale()
        {
            try
            {
               await _lotteryService.StartNewLottery();
                return Ok("New sale started successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
