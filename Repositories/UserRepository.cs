using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinese_sale_api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Data.ChineseSaleDbContext _context;
        public UserRepository(Data.ChineseSaleDbContext context)
        {
            _context = context;
        }
        //register user
        public async Task<User> RegisterUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error registering user: " + ex.Message);
            }
        }

        //get user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        //get user by id
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
