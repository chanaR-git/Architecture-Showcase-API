 using Chinese_sale_api.Data;
using Chinese_sale_api.Models;
using Chinese_sale_api.Utilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Chinese_sale_api.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ChineseSaleDbContext _context;
        private readonly ILogger<DonorRepository> _logger;
        private const string ClassName = nameof(DonorRepository);

        public DonorRepository(ChineseSaleDbContext context, ILogger<DonorRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Donor>> GetDonorsAsync()
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetDonorsAsync), ClassName);
                var donors = await _context.Donors.ToListAsync();
                LoggingHelper.LogMethodWithCount(_logger, nameof(GetDonorsAsync), ClassName, donors.Count);
                return donors;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetDonorsAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor?> GetDonorByIdAsync(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByIdAsync), ClassName, new { DonorId = id });
                var donor = await _context.Donors.FindAsync(id);
                if (donor == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetDonorByIdAsync), ClassName, id.ToString());
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByIdAsync), ClassName);
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetDonorByIdAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor?> GetDonorByEmailAsync(string email)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByEmailAsync), ClassName, new { DonorEmail = email });
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email == email);
                if (donor == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetDonorByEmailAsync), ClassName, email);
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByEmailAsync), ClassName);
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetDonorByEmailAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor?> GetDonorByNameAsync(string name)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByNameAsync), ClassName, new { DonorName = name });
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Name == name);
                if (donor == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetDonorByNameAsync), ClassName, name);
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByNameAsync), ClassName);
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetDonorByNameAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor?> GetDonorByGiftAsync(int giftId)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(GetDonorByGiftAsync), ClassName, new { GiftId = giftId });
                var donor = await _context.Donors
                    .FirstOrDefaultAsync(d => d.MyGifts.Any(g => g.Id == giftId));
                if (donor == null)
                    LoggingHelper.LogNotFound(_logger, nameof(GetDonorByGiftAsync), ClassName, giftId.ToString());
                else
                    LoggingHelper.LogMethodSuccess(_logger, nameof(GetDonorByGiftAsync), ClassName);
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(GetDonorByGiftAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor> AddDonorAsync(Donor donor)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(AddDonorAsync), ClassName, new { donor.Email, donor.Name });
                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
                LoggingHelper.LogCreated(_logger, nameof(AddDonorAsync), ClassName, new { donor.Id, donor.Email, donor.Name });
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(AddDonorAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor> UpdateDonorAsync(Donor donor)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(UpdateDonorAsync), ClassName, new { donor.Id });
                await _context.SaveChangesAsync();
                LoggingHelper.LogUpdated(_logger, nameof(UpdateDonorAsync), ClassName, new { donor.Id, donor.Name });
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(UpdateDonorAsync), ClassName, ex);
                throw;
            }
        }

        public async Task<Donor?> DeleteDonorAsync(int id)
        {
            try
            {
                LoggingHelper.LogMethodStart(_logger, nameof(DeleteDonorAsync), ClassName, new { DonorId = id });
                var donor = await _context.Donors.FindAsync(id);
                if (donor == null)
                {
                    LoggingHelper.LogNotFound(_logger, nameof(DeleteDonorAsync), ClassName, id.ToString());
                    return null;
                }

                _context.Donors.Remove(donor);
                await _context.SaveChangesAsync();
                LoggingHelper.LogDeleted(_logger, nameof(DeleteDonorAsync), ClassName, id.ToString());
                return donor;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogDatabaseError(_logger, nameof(DeleteDonorAsync), ClassName, ex);
                throw;
            }
        }
    }

}