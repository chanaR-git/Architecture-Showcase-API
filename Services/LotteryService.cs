using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Repositories;

namespace Chinese_sale_api.Services
{
    public class LotteryService
    {
        private readonly IPurchasesRepository _purchasesRepository;
        private readonly IGiftRepository _giftRepository;

        public LotteryService(IPurchasesRepository purchasesRepository, IGiftRepository giftRepository)
        {
            _purchasesRepository = purchasesRepository;
            _giftRepository = giftRepository;
        }
        public async Task<ReadUserDto?> RunLottery(string giftName)
        {
            var gift = await _giftRepository.GetGiftByNameAsync(giftName);
            if (gift == null)
            {
                throw new KeyNotFoundException($"Gift with name '{giftName}' not found.");
            }
            int? winnerId = await GetWinnerOfGift(gift.Name);

            var winner = await _giftRepository.UpdateGiftWinnerAsync(gift.Name, winnerId.Value);

            if (winner == null)
                throw new InvalidOperationException("Error updating gift winner.");

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
            var purchases = await _purchasesRepository.GetPurchasesByGiftAsync(giftName);

            if (purchases == null || !purchases.Any())
            {
                return null;
            }
            var winnerIndex = new Random().Next(0, purchases.Count());
            return purchases.ElementAt(winnerIndex).CustomerId;
        }
    }
}
