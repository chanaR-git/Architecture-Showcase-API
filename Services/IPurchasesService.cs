using Chinese_sale_api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chinese_sale_api.Services
{
    public interface IPurchasesService
    {
        Task<IEnumerable<PurchaseDto>> GetPurchasesByGiftAsync(string name);
        Task<IEnumerable<PurchaseDto>> GetBuyersDetailsAsync();
        Task<IEnumerable<PurchaseDto>> GetPurchasesSortedBySellingsAsync();
        Task<IEnumerable<PurchaseDto>> GetPurchasesSortedByPriceAsync();
        Task<PurchaseDto> AddPurchaseAsync(CreatePurchaseDto dto);
    }
}