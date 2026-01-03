using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface ILotteryService
    {
        Task<ReadUserDto?> RunLottery(string giftName);
    }
}