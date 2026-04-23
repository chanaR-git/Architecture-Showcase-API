using Chinese_sale_api.Models;

namespace Chinese_sale_api.Repositories
{
    public interface IUserRepository
    {
        Task<User> RegisterUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
    }
}