using Chinese_sale_api.DTO;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;

namespace Chinese_sale_api.Services
{
    using BCrypt.Net;
    using Microsoft.OpenApi.Extensions;

    public class UserService : IUserService
    {
        private readonly Repositories.IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;
        private const string ClassName = nameof(UserService);

        public UserService(Repositories.IUserRepository userRepository, ITokenService tokenService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _logger = logger;
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
            LoggingHelper.LogMethodStart(_logger, nameof(RegisterUserAsync), ClassName, new { user.Email, user.Name });
            
            if (await _userRepository.GetUserByEmailAsync(user.Email) != null)
            {
                LoggingHelper.LogDuplicateAttempt(_logger, nameof(RegisterUserAsync), ClassName, $"Email: {user.Email}");
                throw new ArgumentException("Email already exists");
            }
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
                LoggingHelper.LogCreated(_logger, nameof(RegisterUserAsync), ClassName, new { created.Id, created.Email });
                return MapToReadUserDto(created);
            }
            catch (Exception ex)
            {
                LoggingHelper.LogUnexpectedError(_logger, nameof(RegisterUserAsync), ClassName, ex);
                throw new Exception(ex.Message);
            }
        }

        //login user
        public async Task<string> LoginUserAsync(string email, string password)
        {
            LoggingHelper.LogMethodStart(_logger, nameof(LoginUserAsync), ClassName, new { UserEmail = email });
            
            var exists = await _userRepository.GetUserByEmailAsync(email);
            if (exists == null || !BCrypt.Verify(password, exists.Password))
            {
                LoggingHelper.LogValidationError(_logger, nameof(LoginUserAsync), ClassName, "Invalid email or password");
                throw new ArgumentException("Invalid email or password");
            }
            
            //token
            string token = _tokenService.GenerateToken(exists.Id, exists.Name, exists.Email, exists.Role, exists.Phone);
            LoggingHelper.LogMethodSuccess(_logger, nameof(LoginUserAsync), ClassName);
            return token;
        }
    }
}
