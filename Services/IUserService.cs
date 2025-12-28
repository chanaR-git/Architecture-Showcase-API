using Chinese_sale_api.DTO;

namespace Chinese_sale_api.Services
{
    public interface IUserService
    {
        Task<string> LoginUserAsync(string email, string password);
        Task<ReadUserDto> RegisterUserAsync(CreateUserDto user);
    }
}