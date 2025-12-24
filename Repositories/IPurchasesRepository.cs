using Chinese_sale_api.Models;

namespace Chinese_sale_api.Repositories
{
    public interface IPurchasesRepository
    {
        Task<Purchase> AddPurchaseAsync(Purchase purchase);
        Task<IEnumerable<Purchase>> GetBuyersDetailsAsync();
        Task<IEnumerable<Purchase>> GetPurchasesByGiftAsync(string name);
        Task<IEnumerable<Purchase>> GetPurchasesSortedByPriceAsync();
        Task<IEnumerable<Purchase>> GetPurchasesSortedBySellingsAsync();
    }
}