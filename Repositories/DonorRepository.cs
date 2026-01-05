 using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Chinese_sale_api.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ChineseSaleDbContext _context;

        public DonorRepository(ChineseSaleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Donor>> GetDonorsAsync()
            => await _context.Donors.ToListAsync();

        public async Task<Donor?> GetDonorByIdAsync(int id)
            => await _context.Donors.FindAsync(id);

        public async Task<Donor?> GetDonorByEmailAsync(string email)
            => await _context.Donors.FirstOrDefaultAsync(d => d.Email == email);

        public async Task<Donor?> GetDonorByNameAsync(string name)
            => await _context.Donors.FirstOrDefaultAsync(d => d.Name == name);

        public async Task<Donor?> GetDonorByGiftAsync(int giftId)
            => await _context.Donors
                .FirstOrDefaultAsync(d => d.MyGifts.Any(g => g.Id == giftId));

        public async Task<Donor> AddDonorAsync(Donor donor)
        {
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();
            return donor;
        }

        public async Task<Donor> UpdateDonorAsync(Donor donor)
        {
            await _context.SaveChangesAsync();
            return donor;
        }

        public async Task<Donor?> DeleteDonorAsync(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null) return null;

            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
            return donor;
        }
    }

}