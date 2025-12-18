using Chinese_sale_api.Models;

namespace Chinese_sale_api.Repositories
{
    public interface IDonorRepository
    {
        Task<Donor> AddDonorAsync(Donor donor);
        Task<Donor?> DeleteDonorAsync(int id);
        Task<Donor?> GetDonorByEmailAsync(string email);
        Task<Donor?> GetDonorByGiftAsync(int giftId);
        Task<Donor?> GetDonorByIdAsync(int id);
        Task<Donor?> GetDonorByNameAsync(string name);
        Task<IEnumerable<Donor>> GetDonorsAsync();
        Task<Donor?> UpdateDonorAsync(int id, Donor updatedDonor);
    }
}