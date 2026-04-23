using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Chinese_sale_api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Data.ChineseSaleDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        private const string ClassName = nameof(UserRepository);

        public UserRepository(Data.ChineseSaleDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        //register user
        public async Task<User> RegisterUserAsync(User user)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(RegisterUserAsync), ClassName, new { user.Email, user.Name });
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                LoggingHelper.LogCreated(_logger, nameof(RegisterUserAsync), ClassName, new { user.Id, user.Email, user.Name });
                return user;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(RegisterUserAsync), ClassName, ex);
                throw new Exception("Error registering user: " + ex.Message);
            }
        }

        //get user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetUserByEmailAsync), ClassName, new { UserEmail = email });
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetUserByEmailAsync), ClassName, email);
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetUserByEmailAsync), ClassName);
                return user;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetUserByEmailAsync), ClassName, ex);
                throw;
            }
        }

        //get user by id
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetUserByIdAsync), ClassName, new { UserId = id });
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetUserByIdAsync), ClassName, id.ToString());
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetUserByIdAsync), ClassName);
                return user;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetUserByIdAsync), ClassName, ex);
                throw;
            }
        }
    }
}
