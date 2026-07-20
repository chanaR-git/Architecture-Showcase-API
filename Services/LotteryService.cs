using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;
using Chinese_sale_api.Utilities;
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
        private readonly IKafkaProducerService _kafkaProducer;

        private const string ClassName = nameof(LotteryService);
        public static int CountLotteries { get; private set; } = 0;

        public LotteryService(IPurchasesRepository purchasesRepository, IGiftRepository giftRepository, ILogger<LotteryService> logger, IUserRepository userRepository, IKafkaProducerService kafkaProducer )
        {
            _purchasesRepository = purchasesRepository;
            _giftRepository = giftRepository;
            _logger = logger;
            _userRepository = userRepository;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<IEnumerable<ReadUserDto?>> RunLottery()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(RunLottery), ClassName, new { LotteryRound = CountLotteries });
            List<ReadUserDto?> winners = new List<ReadUserDto?>();

            var gifts = await _giftRepository.GetGiftsAsync();
            foreach (var gift in gifts)
            {
                int? winnerId = await GetWinnerOfGift(gift.Name);

                if (winnerId == null)
                {
                    LoggingHelper.LogValidationError(_logger, nameof(RunLottery), ClassName, $"No winner for gift: {gift.Name}");
                    continue;
                }

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

            LoggingHelper.LogMethodWithCount(_logger, nameof(RunLottery), ClassName, winners.Count);
            return winners;
        }

        public async Task<ReadUserDto?> RunLottery(string giftName)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(RunLottery), ClassName, new { GiftName = giftName });
            var gift = await _giftRepository.GetGiftByNameAsync(giftName);
            if (gift == null)
            {
                LoggingHelper.LogNotFound(_logger, nameof(RunLottery), ClassName, giftName);
                throw new KeyNotFoundException($"Gift with name '{giftName}' not found.");
            }

            int? winnerId = await GetWinnerOfGift(gift.Name);
            
            if (winnerId == null)
            {
                LoggingHelper.LogValidationError(_logger, nameof(RunLottery), ClassName, $"No purchases for gift: {giftName}");
                return null;
            }

            var winner = await _giftRepository.UpdateGiftWinnerAsync(gift.Name, winnerId.Value);
            
            if (winner == null)
                throw new InvalidOperationException("Error updating gift winner.");

            LoggingHelper.LogMethodSuccess(_logger, nameof(RunLottery), ClassName, new { WinnerName = winner.Name });

            //===========================================Kafka Event Publishing===========================================
            try
            {
                var lotteryEventPayload = new 
                { 
                    Event = "LotteryExecuted", 
                    GiftName = gift.Name, 
                    GiftId = gift.Id, 
                    WinnerId = winner.Id, 
                    WinnerName = winner.Name, 
                    WinnerEmail = winner.Email, 
                    ExecutedAt = DateTime.UtcNow 
                }; 
                string messageKey = gift.Id.ToString(); 
                _logger.LogInformation("Publishing lottery event to Kafka for gift: {GiftName}", giftName); 
                await _kafkaProducer.SendMessageAsync(messageKey, lotteryEventPayload); 
            } 
            catch (Exception ex) 
            { 
                _logger.LogError(ex, "Failed to send lottery event to Kafka for gift: {GiftName}", giftName); 
            } 
            
            return new ReadUserDto
            {
                Id = winner.Id,
                Name = winner.Name,
                Email = winner.Email,
                Phone = winner.Phone
            };
        }
                    

        private async Task<int?> GetWinnerOfGift(string giftName)
        {
            LoggingHelper.LogMethodStart(_logger, $"{nameof(RunLottery)}_SelectWinner", ClassName, new { GiftName = giftName });

            var purchases = await _purchasesRepository.GetPurchasesByGiftAsync(giftName);

            if (purchases == null || !purchases.Any())
            {
                LoggingHelper.LogNotFound(_logger, $"{nameof(RunLottery)}_SelectWinner", ClassName, $"Purchases for {giftName}");
                return null;
            }
            var winnerIndex = new Random().Next(0, purchases.Count());
            var winnerId = purchases.ElementAt(winnerIndex).CustomerId;
            LoggingHelper.LogMethodSuccess(_logger, $"{nameof(RunLottery)}_SelectWinner", ClassName, new { WinnerId = winnerId });
            return winnerId;
        }

        public async Task<List<GiftWinnerDto>> GetAllGiftWinners()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(GetAllGiftWinners), ClassName);

            var gifts = await _giftRepository.GetGiftsAsync();
            if (gifts == null || !gifts.Any())
            {
                LoggingHelper.LogValidationError(_logger, nameof(GetAllGiftWinners), ClassName, "No gifts found");
                throw new InvalidOperationException("No gifts found.");
            }
            var giftWinners = new List<GiftWinnerDto>();

            foreach (var gift in gifts)
            {
                if (gift.WinnerId != null)
                {
                    var winner = await _userRepository.GetUserByIdAsync(gift.WinnerId.Value);
                    if(winner == null)
                    {
                        LoggingHelper.LogNotFound(_logger, nameof(GetAllGiftWinners), ClassName, $"Winner ID: {gift.WinnerId.Value}");
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
              
            LoggingHelper.LogMethodWithCount(_logger, nameof(GetAllGiftWinners), ClassName, giftWinners.Count);
            return giftWinners;
        }

        public async Task<int> StartNewLottery()
        {
            LoggingHelper.LogMethodStart(_logger, nameof(StartNewLottery), ClassName);
            await _giftRepository.StartNewChineseSaleAsync();
            var newCount = ++CountLotteries;
            LoggingHelper.LogMethodSuccess(_logger, nameof(StartNewLottery), ClassName, new { LotteryRound = newCount });
            return newCount;
        }
    }
}
