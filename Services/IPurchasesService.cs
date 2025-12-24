using Chinese_sale_api.DTO;
using Chinese_sale_api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chinese_sale_api.Services
{
    public interface IPurchasesService
    {
        Task<IEnumerable<ReadPurchaseDto>> GetPurchasesByGiftAsync(string name);
        Task<IEnumerable<ReadPurchaseDto>> GetBuyersDetailsAsync();
        Task<IEnumerable<ReadPurchaseDto>> GetPurchasesSortedBySellingsAsync();
        Task<IEnumerable<ReadPurchaseDto>> GetPurchasesSortedByPriceAsync();
        Task<ReadPurchaseDto> AddPurchaseAsync(CreatePurchaseDto dto);
    }
}