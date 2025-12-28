using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;

namespace Chinese_sale_api.Services
{
    using BCrypt.Net;
    using Microsoft.OpenApi.Extensions;

    public class UserService : IUserService
    {
        private readonly Repositories.IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        public UserService(Repositories.IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }
        private ReadUserDto MapToReadUserDto(Models.User user)
        {
            return new ReadUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Phone = user.Phone,
                Role = CustomerRole.User.GetDisplayName()
            };
        }
        //register user
        public async Task<ReadUserDto> RegisterUserAsync(CreateUserDto user)
        {
            if (await _userRepository.GetUserByEmailAsync(user.Email) != null)
                throw new ArgumentException("Email already exists");
            try
            {
                User newUser = new User
                {
                    Name = user.Name,
                    Password = BCrypt.HashPassword(user.Password),
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = CustomerRole.User
                };
                var created = await _userRepository.RegisterUserAsync(newUser);
                return MapToReadUserDto(created);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //login user
        public async Task<string> LoginUserAsync(string email, string password)
        {
            var exists = await _userRepository.GetUserByEmailAsync(email);
            if (exists == null || !BCrypt.Verify(password, exists.Password))
                throw new ArgumentException("Invalid email or password");
            //token
            string token = _tokenService.GenerateToken(exists.Id, exists.Name, exists.Email, exists.Role, exists.Phone);
            return token;
        }
    }
}
