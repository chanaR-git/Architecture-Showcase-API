using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface IZIPService
    {
        void CreateCsvFile(List<GiftWinnerDto> giftWinners,int numLottery ,string csvFilePath);
        void CreateZipFile(string csvFilePath, string zipFilePath);
    }
}