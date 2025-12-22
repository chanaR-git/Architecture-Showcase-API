using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface IDonorService
    {
        Task<ReadDonorDTO?> AddDonorAsync(CreateDonorDTO dto);
        Task<ReadDonorDTO?> DeleteDonorAsync(int id);
        Task<ReadDonorDTO?> GetDonorByEmailAsync(string email);
        Task<ReadDonorDTO?> GetDonorByGiftAsync(int giftId);
        Task<ReadDonorDTO?> GetDonorByIdAsync(int id);
        Task<ReadDonorDTO?> GetDonorByNameAsync(string name);
        Task<IEnumerable<ReadDonorDTO>> GetDonorsAsync();
        Task<ReadDonorDTO?> UpdateDonorAsync(int id, UpdateDonorDTO dto);
    }
}