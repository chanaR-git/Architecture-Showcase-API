using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Chinese_sale_api.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly CheineseSale_DBContext _context;
        public GiftRepository(CheineseSale_DBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Gift>> GetGiftsAsync()
        {
            return await _context.Gifts.Include(g=> g.Category).Include(g => g.Donor).ToListAsync();
        }
        public async Task<Gift> AddGiftAsync(Gift gift)
        {
         
            // Validate foreign keys to avoid FK constraint errors
            if (!await _context.Categories.AnyAsync(c => c.Id == gift.CategoryId))
                throw new ArgumentException($"no such category :(");

            if (!await _context.Donors.AnyAsync(d => d.Id == gift.DonorId))
                throw new ArgumentException($"no such donor :(");

            // Prevent unique index violation on Name (unique index)
            if (await _context.Gifts.AnyAsync(g => g.Name == gift.Name))
                throw new ArgumentException($"A gift with the name '{gift.Name}' already exists.");

            _context.Gifts.Add(gift);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // preserve inner exception for diagnostics
                throw new InvalidOperationException("Error saving Gift to database. See inner exception for details.", ex);
            }

            // Ensure navigation properties are loaded before returning
            await _context.Entry(gift).Reference(g => g.Category).LoadAsync();
            await _context.Entry(gift).Reference(g => g.Donor).LoadAsync();

            return gift;
            //try
            //{
            //    _context.Gifts.Add(Gift);
            //    await _context.SaveChangesAsync();
            //    return Gift;
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}
        }
        public async Task<Gift> UpdateGiftAsync(string name, Gift updatedGift)
        {
            var gift = await _context.Gifts.FindAsync(name);
            if (gift == null)
            {
                throw new KeyNotFoundException($"no such gift :(");
            }

            // Validate foreign keys to avoid FK constraint errors
            if (!await _context.Categories.AnyAsync(c => c.Id == updatedGift.CategoryId))
                throw new ArgumentException($"no such category :(");

            if (!await _context.Donors.AnyAsync(d => d.Id == updatedGift.DonorId))
                throw new ArgumentException($"no such donor :(");

            // Prevent unique index violation on Name (unique index)
            if (await _context.Gifts.AnyAsync(g => g.Name == updatedGift.Name))
                throw new ArgumentException($"A gift with the name '{updatedGift.Name}' already exists.");
            
            gift.Name = updatedGift.Name;
            gift.Price = updatedGift.Price;
            gift.Description = updatedGift.Description;
            gift.CategoryId = updatedGift.CategoryId;
            gift.ImagePath = updatedGift.ImagePath;
            //תורם א"א לשנות אחרי תרומה
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // preserve inner exception for diagnostics
                throw new InvalidOperationException("Error saving Gift to database. See inner exception for details.", ex);
            }
            return gift;
        }
        public async Task<Gift?> DeleteGiftAsync(string name)
        {
            var gift = await _context.Gifts.FindAsync(name);
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
            return await _context.Gifts.FirstOrDefaultAsync(d => d.Name == name);
        }
        public async Task<IEnumerable<Gift>?> GetGiftByDonorAsync(string name)
        {
            return await _context.Gifts.Include(g=>g.Donor).Where(g => g.Donor.Name == name).ToListAsync();
        }
        public async Task<IEnumerable<Gift>?> getByNumBuyers(int count)
        {
            return await _context.Gifts.Include(g => g.Purchases).Where(g => g.Purchases.Count == count).ToListAsync();
        }

    }
}