using Chinese_sale_api.Controllers;
using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Chinese_sale_api.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly ChineseSaleDbContext _context;
        public GiftRepository(ChineseSaleDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Gift>> GetGiftsAsync()
        {
            return await _context.Gifts.AsNoTracking().Include(g=> g.Category).Include(g => g.Donor).ToListAsync();
        }
        public async Task<Gift> AddGiftAsync(Gift gift)
        {
            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();
            return gift;
        }
        public async Task<Gift?> UpdateGiftAsync(Gift gift)
        {
            await _context.SaveChangesAsync();
            return gift;
        }
        //update gift winner
        public async Task<User?> UpdateGiftWinnerAsync(string name, int winnerId)
        {
            var gift = await _context.Gifts.FirstOrDefaultAsync(g => g.Name == name);
            if (gift == null)
            {
                return null;
            }
            gift.WinnerId = winnerId;
            await _context.SaveChangesAsync();
            var winner = await _context.Users.FindAsync(winnerId);
            return winner;
        }
        public async Task<Gift?> DeleteGiftAsync(string name)
        {
            var gift = await _context.Gifts.FirstOrDefaultAsync(g => g.Name == name);
            if (gift == null)
            {
                return null;
            }
            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();
            return gift;
        }
        public async Task<Gift?> GetGiftByNameAsync(string name)
        {
            return await _context.Gifts
                        .Include(g=>g.Category)
                        .Include(g=>g.Donor)
                        .FirstOrDefaultAsync(d => d.Name == name);
        }
        public async Task<IEnumerable<Gift>> GetGiftByDonorAsync(string name)
        {
            return await _context.Gifts
                           .Include(g=>g.Donor)
                           .Where(g => g.Donor.Name == name)
                           .ToListAsync();
        }
        public async Task<IEnumerable<Gift>> getByNumBuyers(int count)
        {
            return await _context.Gifts
                        .Include(g => g.Purchases)
                        .Where(g => g.Purchases !=null && g.Purchases.Count == count)
                        .ToListAsync();
        }

    }
}