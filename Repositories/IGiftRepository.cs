using Chinese_sale_api.Models;

namespace Chinese_sale_api.Repositories
{
    public interface IGiftRepository
    {
        Task<Gift> AddGiftAsync(Gift Gift);
        Task<Gift> UpdateGiftAsync(string name, Gift updatedGift);
        Task<Gift?> DeleteGiftAsync(string name);
        Task<IEnumerable<Gift>?> getByNumBuyers(int count);
        Task<IEnumerable<Gift>?> GetGiftByDonorAsync(string name);
        Task<Gift?> GetGiftByNameAsync(string name);
        Task<IEnumerable<Gift>> GetGiftsAsync();
    }
}