using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface IGiftService
    {
        Task<PagedResult<ReadGiftDTO>> GetGiftsPagedAsync(PaginationParams @params);
        Task<ReadGiftDTO?> AddGiftAsync(CreateGiftDTO g);
        Task<ReadGiftDTO?> DeleteGiftAsync(string name);
        Task<IEnumerable<ReadGiftDTO>> getByNumBuyersAsync(int count);
        Task<IEnumerable<ReadGiftDTO>?> GetGiftByDonorAsync(string name);
        Task<ReadGiftDTO?> GetGiftByNameAsync(string name);
        Task<IEnumerable<ReadGiftDTO>> GetGiftsAsync();
        Task<ReadGiftDTO?> UpdateGiftAsync(string name, UpdateGiftDTO updatedGift);
        Task<string?> GetWinnerOfGift(string name);
        Task<ImageUploadResponseDTO> UploadGiftImageAsync(int giftId, IFormFile file);
        Task<FileStream?> DownloadGiftImageAsync(int giftId);
    }
}