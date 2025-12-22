using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Chinese_sale_api.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly CheineseSale_DBContext _context;
        public DonorRepository(CheineseSale_DBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Donor>> GetDonorsAsync()
        {
            return await _context.Donors.ToListAsync();
        }
        public async Task<Donor?> GetDonorByIdAsync(int id)
        {
            return await _context.Donors.FindAsync(id);
        }
        public async Task<Donor?> GetDonorByEmailAsync(string email)
        {
            return await _context.Donors.FirstOrDefaultAsync(d => d.Email == email);
        }
        public async Task<Donor?> GetDonorByNameAsync(string name)
        {
            return await _context.Donors.FirstOrDefaultAsync(d => d.Name == name);
        }
        public async Task<Donor?> GetDonorByGiftAsync(int giftId)
        {
            return await _context.Donors.Include(d => d.MyGifts).FirstOrDefaultAsync(d => d.MyGifts.Any(g => g.Id == giftId));
        }

        public async Task<Donor?> AddDonorAsync(Donor donor)
        {
            try
            {
                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
                return donor;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Donor?> UpdateDonorAsync(int id, Donor updatedDonor)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
            {
                return null;
            }
            try
            {
                donor.Name = updatedDonor.Name;
                donor.Email = updatedDonor.Email;
                donor.Phone = updatedDonor.Phone;
                await _context.SaveChangesAsync();
                return donor;
            }
            catch (Exception ex) { return null; }
        }
        public async Task<Donor?> DeleteDonorAsync(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
            {
                return null;
            }
            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
            return donor;
        }
    }
}