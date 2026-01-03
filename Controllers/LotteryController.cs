using Chinese_sale_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Chinese_sale_api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[apicontroller]")]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        public LotteryController(ILotteryService lotteryService)
        {
            _lotteryService = lotteryService;
        }

        [HttpPost]
        public async Task<IActionResult> RunLottery(string giftName)
        {
            try
            {
                var result = await _lotteryService.RunLottery(giftName);
                return Ok(result);
            }
            catch(Exception ex) {
                return BadRequest(ex.Message);
            }

        }

    }
}
