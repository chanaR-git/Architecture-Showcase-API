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
        private readonly IZIPService _zipService;
        public LotteryController(ILotteryService lotteryService, IZIPService zipService)
        {
            _lotteryService = lotteryService;
            _zipService = zipService;
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


        [HttpGet("download-winners-zip")]
        public async Task<IActionResult> DownloadGiftWinnersAsZip()
        {
            var giftWinners = await _lotteryService.GetAllGiftWinners();

            if (giftWinners == null || !giftWinners.Any())
            {
                return NotFound("No winners data to download.");
            }

            var csvFileName = "gift_winners.csv";
            var csvFilePath = Path.Combine(Path.GetTempPath(), csvFileName);
            _zipService.CreateCsvFile(giftWinners,LotteryService.CountLotteries, csvFilePath);

            var zipFileName = "gift_winners.zip";
            var zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);
            _zipService.CreateZipFile(csvFilePath, zipFilePath);

            System.IO.File.Delete(csvFilePath);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);

            // Use the correct overload for ControllerBase.File
            return File(fileBytes, "application/zip", zipFileName);
        }
    }
}
