using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface ILotteryService
    {
        Task<IEnumerable<ReadUserDto?>> RunLottery();
        Task<List<GiftWinnerDto>> GetAllGiftWinners();
        Task<int> StartNewLottery();
    }
}