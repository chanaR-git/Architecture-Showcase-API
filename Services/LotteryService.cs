using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using CsvHelper;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Chinese_sale_api.Services
{
    public class LotteryService : ILotteryService
    {
        private readonly IPurchasesRepository _purchasesRepository;
        private readonly IGiftRepository _giftRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LotteryService> _logger;
        //private static int countLoterries = 0;
        public static int CountLotteries { get; private set; } = 0;

        public LotteryService(IPurchasesRepository purchasesRepository, IGiftRepository giftRepository, ILogger<LotteryService> logger, IUserRepository userRepository)
        {
            _purchasesRepository = purchasesRepository;
            _giftRepository = giftRepository;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ReadUserDto?>> RunLottery()
        {
            _logger.LogInformation("Starting lottery run number {CountLoterries}", CountLotteries);
            List<ReadUserDto?> winners = new List<ReadUserDto?>();

            var gifts = await _giftRepository.GetGiftsAsync();
            foreach (var gift in gifts)
            {

                int? winnerId = await GetWinnerOfGift(gift.Name);

                if (winnerId == null)
                    continue;

                var winner = await _giftRepository.UpdateGiftWinnerAsync(gift.Name, winnerId.Value);

                if (winner == null)
                    throw new InvalidOperationException("Error updating gift winner.");

                winners.Add(new ReadUserDto
                {
                    Id = winner.Id,
                    Name = winner.Name,
                    Email = winner.Email,
                    Phone = winner.Phone
                });
           
            }

            return winners;
        }

        private async Task<int?> GetWinnerOfGift(string giftName)
        {
            _logger.LogInformation("Selecting a winner for gift: {GiftName}", giftName);

            var purchases = await _purchasesRepository.GetPurchasesByGiftAsync(giftName);

            if (purchases == null || !purchases.Any())
            {
                _logger.LogWarning("No purchases found for gift: {GiftName}. Cannot select a winner.", giftName);
                return null;
            }
            var winnerIndex = new Random().Next(0, purchases.Count());
            return purchases.ElementAt(winnerIndex).CustomerId;
        }

        public async Task<List<GiftWinnerDto>> GetAllGiftWinners()
        {
            _logger.LogInformation("Fetching all gifts and their winners.");

            // קבלת כל המתנות מהמאגר
            var gifts = await _giftRepository.GetGiftsAsync(); 
            if (gifts == null || !gifts.Any())
            {
                _logger.LogWarning("No gifts found.");
                throw new InvalidOperationException("No gifts found.");
            }
            var giftWinners = new List<GiftWinnerDto>();

            // מעבדים כל מתנה ומקבלים את פרטי הזוכה שלה
            foreach (var gift in gifts)
            {
                if (gift.WinnerId != null)
                {
                    // אם כבר יש למתנה זוכה, מקבלים את פרטי הזוכה
                    var winner = await _userRepository.GetUserByIdAsync(gift.WinnerId.Value);
                    if(winner == null)
                    {
                        _logger.LogWarning("Winner with ID {WinnerId} not found for gift {GiftName}.", gift.WinnerId.Value, gift.Name);
                        throw new InvalidOperationException($"Winner with ID {gift.WinnerId.Value} not found for gift {gift.Name}.");
                    }

                    giftWinners.Add(new GiftWinnerDto
                    {
                        GiftName = gift.Name,
                        WinnerName = winner.Name,
                        WinnerEmail = winner.Email,
                        WinnerPhone = winner.Phone
                    });
                }
            }
              
            return giftWinners;
        }


        public async Task<int> StartNewLottery()
        {
            _logger.LogInformation("Starting a new lottery round.");
            await _giftRepository.StartNewChineseSaleAsync();
            return ++CountLotteries;
        }
    }
}
